using UnityEngine;

namespace RootMotion.FinalIK
{

    /// <summary>
    /// Relaxes the twist rotation if the TwistSolver transforms relative to their parent and a child Transforms, using their initial rotations as the most relaxed pose.
    /// </summary>
    [RequireComponent(typeof(VRIK))]
    public class TwistRelaxer : MonoBehaviour
    {
        private IK _ik;

        [Tooltip("If using multiple solvers, add them in inverse hierarchical order - first forearm roll bone, then forearm bone and upper arm bone.")]
        public TwistSolver[] twistSolvers = new TwistSolver[0];

        private void Awake()
        {
            _ik = GetComponent<VRIK>();
        }

        public void Start()
        {
            if (twistSolvers.Length == 0)
            {
                Debug.LogError("TwistRelaxer has no TwistSolvers. TwistRelaxer.cs was restructured for FIK v2.0 to support multiple relaxers on the same body part and TwistRelaxer components need to be set up again, sorry for the inconvenience!", transform);
                return;
            }

            foreach (TwistSolver twistSolver in twistSolvers)
            {
                twistSolver.Initiate();
            }

            _ik.GetIKSolver().OnPostUpdate += OnPostUpdate;
        }

        private void Update()
        {
            if (!_ik.fixTransforms) {return;}
            foreach (TwistSolver twistSolver in twistSolvers)
            {
                twistSolver.FixTransforms();
            }
        }

        private void OnPostUpdate()
        {
            foreach (TwistSolver twistSolver in twistSolvers)
            { 
                twistSolver.Relax();
            }
        }

        private void LateUpdate()
        {
            foreach (TwistSolver twistSolver in twistSolvers)
            {
                twistSolver.Relax();
            }
        }

        private void OnDestroy()
        {
            if (_ik != null) _ik.GetIKSolver().OnPostUpdate -= OnPostUpdate;
        }
    }
}
