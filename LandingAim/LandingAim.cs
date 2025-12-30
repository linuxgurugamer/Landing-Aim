using UnityEngine;

namespace LandingAim
{
    public class LandingAim : PartModule
    {
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Landing Aim", isPersistant = true)]
        [UI_Toggle(controlEnabled = true, enabledText = "On", disabledText = "Off", scene = UI_Scene.Flight)]
        public bool IsLandingAim;

        private int _pointsCount = 128;
        private float _lineWidth = 0.1f;

        private Texture2D _crossTexture;

        private Transform _crossTransform;
        private Transform CrossTransform
        {
            get
            {
                if (_crossTransform == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    var r = obj.GetComponent<Renderer>();
                    var mat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                    mat.SetTexture("_MainTex", _crossTexture);
                    r.sharedMaterial = mat;
                    var col = obj.GetComponent<Collider>();
                    col.enabled = false;
                    _crossTransform = obj.transform;
                }
                return _crossTransform;
            }
        }

        private readonly LayerMask _targetLayer = LayerMask.GetMask("TerrainColliders", "PhysicalObjects", "EVA", "Local Scenery", "Water");
        private LineRenderer Line { get; set; }

        public void Start()
        {
            _crossTexture = GameDatabase.Instance.GetTexture("LandingAim/AimCross", false);
            if (_crossTexture == null)
                Debug.LogError("AimCross not found");
            Line = gameObject.AddComponent<LineRenderer>();
            Line.useWorldSpace = true;
            Line.enabled = IsLandingAim;
            //Line.SetVertexCount(_pointsCount);
            Line.positionCount = _pointsCount;
            _pointsCount = Line.positionCount;
            //Line.SetWidth(_lineWidth, _lineWidth);
            Line.startWidth = _lineWidth;
            Line.endWidth = _lineWidth;
            Line.sharedMaterial = Resources.Load("DefaultLine3D") as Material;
            Line.material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f));
            if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onVesselChange.Add(onVesselChange);
                GameEvents.onVesselSwitching.Add(onVesselSwitching);
                GameEvents.onVesselLoaded.Add(onVesselLoaded);
                GameEvents.onVesselCreate.Add(onVesselCreate);
                GameEvents.onNewVesselCreated.Add(onNewVesselCreated);
                GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            }
            UpdatePAW(this.vessel);
        }

        void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onVesselSwitching.Remove(onVesselSwitching);
            GameEvents.onVesselLoaded.Remove(onVesselLoaded);
            GameEvents.onVesselCreate.Remove(onVesselCreate);
            GameEvents.onNewVesselCreated.Remove(onNewVesselCreated);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }

        void OnGameSettingsApplied()
        {
            onVesselChange(FlightGlobals.ActiveVessel);
        }
        void onVesselCreate(Vessel v)
        {
            onVesselChange(v);
        }
        void onNewVesselCreated(Vessel v)
        {
            onVesselChange(v);
        }
        void onVesselSwitching(Vessel from, Vessel to)
        {
            onVesselChange(to);
        }

        void onVesselLoaded(Vessel v)
        {
            onVesselChange(v);
        }

        void onVesselChange(Vessel v)
        {
            if (v == FlightGlobals.ActiveVessel)
            {
                UpdatePAW(v);
            }
        }
        void UpdatePAW(Vessel v)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().needsMinimumPilotLevel)
            {
                int hpl = VesselPilotAbility.GetHighestPilotLevel(v);
                if (hpl < HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().minPilotLevel)
                {

                    DisablePAW();
                    return;
                }
            }

            int mpl = VesselPilotAbility.GetHighestPilotSasLevel(v);
            if (HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().needsFullSAS && mpl < 4)
            {
                DisablePAW();
                return;
            }
            if (HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().needsTargetAntitarget && mpl < 3)
            {
                DisablePAW();
                return;
            }
            if (HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().needsNormalAntinormal && mpl < 2)
            {
                DisablePAW();
                return;
            }
            if (HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().needsProgradeRetrograd && mpl < 1)
            {
                DisablePAW();
                return;
            }
            EnablePAW();
        }



        void DisablePAW()
        {
            Fields["IsLandingAim"].guiActive = false;
        }
        void EnablePAW()
        {
            Fields["IsLandingAim"].guiActive = true;
        }

        private void FixedUpdate()
        {
            if (IsLandingAim)
            {
                if (!Line.enabled) Line.enabled = true;

                var pos = FlightGlobals.ActiveVessel.transform.position;
                var dragVector = FlightGlobals.activeTarget.dragVector;
                var grav = FlightGlobals.ActiveVessel.graviticAcceleration;
                var drawTime = 0;

                RaycastHit hit;

                for (var i = 1; i < 10; i++)
                {
                    drawTime++;
                    var lastPos = pos;
                    dragVector += grav * i;
                    pos += dragVector * i;
                    if (Physics.Linecast(lastPos, pos, out hit, _targetLayer)) break;
                }

                dragVector = FlightGlobals.activeTarget.dragVector;
                pos = FlightGlobals.ActiveVessel.transform.position;

                Vector3[] poses = new Vector3[_pointsCount];

                var interval = (float)drawTime / poses.Length;

                var checkContact = true;
                poses[0] = pos;
                for (var i = 1; i < poses.Length; i++)
                {
                    var lastPos = pos;
                    dragVector += grav * (i * interval);
                    pos += dragVector * (i * interval);
                    poses[i] = pos;
                    if (checkContact && Physics.Linecast(lastPos, pos, out hit, _targetLayer))
                    {
                        checkContact = false;
                        CrossTransform.position = hit.point + hit.normal * 0.16f;
                        CrossTransform.localEulerAngles = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
                    }
                }
                Line.SetPositions(poses);
            }
            else
            {
                if (Line.enabled) Line.enabled = false;
                if (_crossTransform != null) _crossTransform.gameObject.DestroyGameObject();
            }
        }
    }
}