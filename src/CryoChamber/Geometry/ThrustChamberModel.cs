using System;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;
using CryoChamber.Configuration;
using CryoChamber.Physics;

namespace CryoChamber.Geometry
{
    /// <summary>
    /// Master Computational Engineering Model for the Regeneratively Cooled Cryogenic Thrust Chamber.
    /// Orchestrates first-principles thermohydraulic calculations, contour modeling,
    /// conformal cooling channel generation, structural jacket generation, manifold integration,
    /// and final Boolean synthesis.
    /// </summary>
    public class ThrustChamberModel
    {
        protected ChamberParameters          m_oParams;
        protected ThermalHydraulics          m_oAeroThermal;
        protected NozzleContour              m_oContour;
        protected CoolingChannelGenerator   m_oChannelGen;
        protected ManifoldBuilder            m_oManifoldGen;

        public ThrustChamberModel(ChamberParameters oParams)
        {
            m_oParams       = oParams;
            m_oAeroThermal  = new ThermalHydraulics(m_oParams);
            m_oContour      = new NozzleContour(m_oParams);
            m_oChannelGen   = new CoolingChannelGenerator(m_oParams, m_oAeroThermal, m_oContour);
            m_oManifoldGen  = new ManifoldBuilder(m_oParams, m_oContour);
        }

        /// <summary>
        /// Generates the inner hot-gas liner wall.
        /// </summary>
        public Voxels voxConstructHotWall()
        {
            LocalFrame oFrame   = new LocalFrame(Vector3.Zero);
            float fLength       = m_oParams.m_fTotalLengthMM;

            BasePipe oHotWall = new BasePipe(
                oFrame,
                fLength,
                m_oParams.m_fChamberRadiusMM,
                m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM
            );
            oHotWall.SetLengthSteps(120);
            oHotWall.SetPolarSteps(60);
            oHotWall.SetRadialSteps(10);

            SurfaceModulation oInnerMod = new SurfaceModulation(new LineModulation(m_oContour.fGetInnerRadiusAtRatio));
            SurfaceModulation oOuterMod = new SurfaceModulation(new LineModulation(m_oContour.fGetHotWallRadiusAtRatio));
            oHotWall.SetRadius(oInnerMod, oOuterMod);

            return oHotWall.voxConstruct();
        }

        /// <summary>
        /// Generates the outer structural jacket containment wall.
        /// </summary>
        public Voxels voxConstructStructuralJacket()
        {
            LocalFrame oFrame   = new LocalFrame(Vector3.Zero);
            float fLength       = m_oParams.m_fTotalLengthMM;

            BasePipe oJacket = new BasePipe(
                oFrame,
                fLength,
                m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightChamberMM,
                m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightChamberMM + m_oParams.m_fJacketThicknessMM
            );
            oJacket.SetLengthSteps(120);
            oJacket.SetPolarSteps(60);
            oJacket.SetRadialSteps(10);

            SurfaceModulation oInnerMod = new SurfaceModulation(new LineModulation(fGetChannelOuterRadiusRatio));
            SurfaceModulation oOuterMod = new SurfaceModulation(new LineModulation(fGetJacketOuterRadiusRatio));
            oJacket.SetRadius(oInnerMod, oOuterMod);

            return oJacket.voxConstruct();
        }

        /// <summary>
        /// Generates the top injector interface flange.
        /// </summary>
        public Voxels voxConstructInjectorFlange()
        {
            LocalFrame oFrame = new LocalFrame(new Vector3(0, 0, 0));
            float fInnerR = m_oParams.m_fChamberRadiusMM;
            float fOuterR = m_oParams.m_fChamberRadiusMM + 22.0f;
            float fThickness = 7.0f;

            BasePipe oFlange = new BasePipe(oFrame, fThickness, fInnerR, fOuterR);
            oFlange.SetRadialSteps(20);
            oFlange.SetPolarSteps(60);
            return oFlange.voxConstruct();
        }

        /// <summary>
        /// Synthesizes the full solid manufacturing geometry:
        /// 1. Combines hotwall liner, structural outer jacket, injector flange, and manifolds.
        /// 2. Combines all 64 cooling channels and internal manifold cavities.
        /// 3. Subtracts the fluid passages to yield the final monolithic 3D-printable chamber.
        /// </summary>
        public (Voxels voxFinalChamber, Voxels voxFluidDomain) voxBuildCompleteChamber()
        {
            Console.WriteLine("--> [1/5] Building Hot-gas Inner Liner...");
            Voxels voxHotWall = voxConstructHotWall();

            Console.WriteLine("--> [2/5] Building Outer Structural Jacket & Flanges...");
            Voxels voxJacket = voxConstructStructuralJacket();
            Voxels voxFlange = voxConstructInjectorFlange();
            Voxels voxManifoldsOuter = m_oManifoldGen.voxConstructManifoldsOuter();

            // Unite solid body
            Voxels voxSolidBody = voxHotWall;
            voxSolidBody.BoolAdd(voxJacket);
            voxSolidBody.BoolAdd(voxFlange);
            voxSolidBody.BoolAdd(voxManifoldsOuter);

            Console.WriteLine($"--> [3/5] Generating {m_oParams.m_nChannelCount} Conformal Cooling Channels...");
            Voxels voxChannels = m_oChannelGen.voxConstructChannels();

            Console.WriteLine("--> [4/5] Constructing Manifold Fluid Cavities...");
            Voxels voxManifoldsCavity = m_oManifoldGen.voxConstructManifoldsCavity();
            Voxels voxFluidDomain = voxChannels;
            voxFluidDomain.BoolAdd(voxManifoldsCavity);

            Console.WriteLine("--> [5/5] Executing Boolean Voxel Carve (Solid - Fluid)...");
            Voxels voxFinalChamber = voxSolidBody;
            voxFinalChamber.BoolSubtract(voxFluidDomain);

            Console.WriteLine("--> Monolithic Thrust Chamber Synthesis Complete!");
            return (voxFinalChamber, voxFluidDomain);
        }

        /// <summary>
        /// Creates a 90-degree angular cutaway of the chamber to reveal the intricate
        /// internal conformal cooling channels, rib profiles, and throat passage for inspection.
        /// </summary>
        public Voxels voxCreateCutaway(Voxels voxModel)
        {
            // A quadrant bounding box from x: [0, 200], y: [0, 200], z: [-10, 250]
            LocalFrame oFrame = new LocalFrame(new Vector3(100.0f, 100.0f, m_oParams.m_fTotalLengthMM * 0.5f));
            BaseBox oCutBox = new BaseBox(oFrame, 220.0f, 220.0f, m_oParams.m_fTotalLengthMM + 30.0f);
            Voxels voxCutBox = oCutBox.voxConstruct();

            Voxels voxCutaway = new Voxels(voxModel);
            voxCutaway.BoolSubtract(voxCutBox);
            return voxCutaway;
        }

        // --- Helper Callbacks ---

        protected float fGetChannelOuterRadiusRatio(float fLengthRatio)
        {
            float fHeight = m_oAeroThermal.fGetChannelHeightMM(fLengthRatio);
            return m_oContour.fGetChannelOuterRadiusAtRatio(fLengthRatio, fHeight);
        }

        protected float fGetJacketOuterRadiusRatio(float fLengthRatio)
        {
            return fGetChannelOuterRadiusRatio(fLengthRatio) + m_oParams.m_fJacketThicknessMM;
        }
    }
}
