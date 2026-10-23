using System;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;
using CryoChamber.Configuration;

namespace CryoChamber.Geometry
{
    /// <summary>
    /// Builds the cryogenic inlet and outlet distribution manifolds.
    /// Manifolds wrap around the nozzle skirt and chamber head to distribute
    /// and collect liquid hydrogen with minimal stagnation pressure loss.
    /// </summary>
    public class ManifoldBuilder
    {
        protected ChamberParameters m_oParams;
        protected NozzleContour     m_oContour;

        public ManifoldBuilder(ChamberParameters oParams, NozzleContour oContour)
        {
            m_oParams   = oParams;
            m_oContour  = oContour;
        }

        /// <summary>
        /// Generates the outer solid geometry of the inlet and outlet manifold tori and inlet/outlet feed ports.
        /// </summary>
        public Voxels voxConstructManifoldsOuter()
        {
            // 1. Outlet Manifold (Top, at injector head, z = 6 mm)
            float fZ_Outlet = 6.0f;
            float fR_OutletMajor = m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightChamberMM + 2.0f;
            float fR_OutletTube = m_oParams.m_fManifoldMajorRadiusMM;

            LocalFrame oOutletFrame = new LocalFrame(new Vector3(0, 0, fZ_Outlet));
            BaseRing oOutletRing = new BaseRing(oOutletFrame, fR_OutletMajor, fR_OutletTube);
            oOutletRing.SetPolarSteps(60);
            oOutletRing.SetRadialSteps(40);
            Voxels voxResult = oOutletRing.voxConstruct();

            // Outlet Feed Pipe (extends radially along +X)
            LocalFrame oOutletPipeFrame = new LocalFrame(
                new Vector3(fR_OutletMajor, 0, fZ_Outlet),
                Vector3.UnitX, // Normal along +X
                Vector3.UnitZ
            );
            BaseCylinder oOutletPipe = new BaseCylinder(oOutletPipeFrame, 25.0f, m_oParams.m_fOutletPipeDiameterMM * 0.5f + 1.8f);
            oOutletPipe.SetRadialSteps(30);
            voxResult.BoolAdd(oOutletPipe.voxConstruct());

            // 2. Inlet Manifold (Bottom, at nozzle exit skirt, z = TotalLength - 12 mm)
            float fZ_Inlet = m_oParams.m_fTotalLengthMM - 12.0f;
            float fR_InletMajor = m_oContour.fGetInnerRadiusAtZ(fZ_Inlet) + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightExitMM + 2.0f;
            float fR_InletTube = m_oParams.m_fManifoldMajorRadiusMM * 0.85f;

            LocalFrame oInletFrame = new LocalFrame(new Vector3(0, 0, fZ_Inlet));
            BaseRing oInletRing = new BaseRing(oInletFrame, fR_InletMajor, fR_InletTube);
            oInletRing.SetPolarSteps(60);
            oInletRing.SetRadialSteps(40);
            voxResult.BoolAdd(oInletRing.voxConstruct());

            // Inlet Feed Pipe (extends radially along +X)
            LocalFrame oInletPipeFrame = new LocalFrame(
                new Vector3(fR_InletMajor, 0, fZ_Inlet),
                Vector3.UnitX,
                Vector3.UnitZ
            );
            BaseCylinder oInletPipe = new BaseCylinder(oInletPipeFrame, 25.0f, m_oParams.m_fInletPipeDiameterMM * 0.5f + 1.8f);
            oInletPipe.SetRadialSteps(30);
            voxResult.BoolAdd(oInletPipe.voxConstruct());

            return voxResult;
        }

        /// <summary>
        /// Generates the hollow internal fluid channels for the manifolds.
        /// These are subtracted from the solid assembly to form flow passages.
        /// </summary>
        public Voxels voxConstructManifoldsCavity()
        {
            // 1. Outlet Cavity
            float fZ_Outlet = 6.0f;
            float fR_OutletMajor = m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightChamberMM + 2.0f;
            float fR_OutletInnerTube = m_oParams.m_fManifoldMajorRadiusMM - m_oParams.m_fManifoldWallMM;

            LocalFrame oOutletFrame = new LocalFrame(new Vector3(0, 0, fZ_Outlet));
            BaseRing oOutletRing = new BaseRing(oOutletFrame, fR_OutletMajor, fR_OutletInnerTube);
            oOutletRing.SetPolarSteps(60);
            oOutletRing.SetRadialSteps(40);
            Voxels voxCavity = oOutletRing.voxConstruct();

            // Outlet pipe flow bore
            LocalFrame oOutletPipeFrame = new LocalFrame(
                new Vector3(fR_OutletMajor - 2.0f, 0, fZ_Outlet),
                Vector3.UnitX,
                Vector3.UnitZ
            );
            BaseCylinder oOutletBore = new BaseCylinder(oOutletPipeFrame, 30.0f, m_oParams.m_fOutletPipeDiameterMM * 0.5f);
            oOutletBore.SetRadialSteps(30);
            voxCavity.BoolAdd(oOutletBore.voxConstruct());

            // 2. Inlet Cavity
            float fZ_Inlet = m_oParams.m_fTotalLengthMM - 12.0f;
            float fR_InletMajor = m_oContour.fGetInnerRadiusAtZ(fZ_Inlet) + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightExitMM + 2.0f;
            float fR_InletInnerTube = (m_oParams.m_fManifoldMajorRadiusMM * 0.85f) - m_oParams.m_fManifoldWallMM;

            LocalFrame oInletFrame = new LocalFrame(new Vector3(0, 0, fZ_Inlet));
            BaseRing oInletRing = new BaseRing(oInletFrame, fR_InletMajor, fR_InletInnerTube);
            oInletRing.SetPolarSteps(60);
            oInletRing.SetRadialSteps(40);
            voxCavity.BoolAdd(oInletRing.voxConstruct());

            // Inlet pipe flow bore
            LocalFrame oInletPipeFrame = new LocalFrame(
                new Vector3(fR_InletMajor - 2.0f, 0, fZ_Inlet),
                Vector3.UnitX,
                Vector3.UnitZ
            );
            BaseCylinder oInletBore = new BaseCylinder(oInletPipeFrame, 30.0f, m_oParams.m_fInletPipeDiameterMM * 0.5f);
            oInletBore.SetRadialSteps(30);
            voxCavity.BoolAdd(oInletBore.voxConstruct());

            return voxCavity;
        }
    }
}
