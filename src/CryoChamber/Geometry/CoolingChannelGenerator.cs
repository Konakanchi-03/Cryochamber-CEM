using System;
using System.Numerics;
using System.Collections.Generic;
using PicoGK;
using Leap71.ShapeKernel;
using CryoChamber.Configuration;
using CryoChamber.Physics;

namespace CryoChamber.Geometry
{
    /// <summary>
    /// Generates the conformal variable-aspect-ratio cooling channel network.
    /// Each channel is constructed as a conformal BasePipeSegment modulated along
    /// the nozzle length according to heat flux, throat constriction, and DfAM rules.
    /// </summary>
    public class CoolingChannelGenerator
    {
        protected ChamberParameters  m_oParams;
        protected ThermalHydraulics  m_oAeroThermal;
        protected NozzleContour      m_oContour;

        public CoolingChannelGenerator(
            ChamberParameters oParams,
            ThermalHydraulics oAeroThermal,
            NozzleContour oContour)
        {
            m_oParams       = oParams;
            m_oAeroThermal  = oAeroThermal;
            m_oContour      = oContour;
        }

        /// <summary>
        /// Assembles all conformal cooling channels into a single unified mesh.
        /// Combining channels into a unified mesh before voxelization reduces computation time significantly.
        /// </summary>
        public Mesh mshConstructChannels()
        {
            Mesh mshAllChannels = new Mesh();
            uint nChannels      = m_oParams.m_nChannelCount;
            float fLength       = m_oParams.m_fTotalLengthMM;
            LocalFrame oFrame   = new LocalFrame(Vector3.Zero);

            for (uint i = 0; i < nChannels; i++)
            {
                float fPhiMidVal = (float)i * (2.0f * MathF.PI / nChannels);

                // Define mid angle and angular span modulations
                LineModulation oPhiMid = new LineModulation(fPhiMidVal);
                LineModulation oPhiRange = new LineModulation(fGetChannelPhiRange);

                // Construct segment with baseline dimensions
                BasePipeSegment oChannel = new BasePipeSegment(
                    oFrame,
                    fLength,
                    m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM,
                    m_oParams.m_fChamberRadiusMM + m_oParams.m_fHotWallThicknessMM + m_oParams.m_fChannelHeightChamberMM,
                    oPhiMid,
                    oPhiRange,
                    BasePipeSegment.EMethod.MID_RANGE
                );

                // Resolution settings: 80 axial steps, 5 polar, 5 radial
                oChannel.SetLengthSteps(80);
                oChannel.SetPolarSteps(5);
                oChannel.SetRadialSteps(5);

                // Conformal radial modulation: inner radius = hot wall, outer radius = hot wall + height(t)
                SurfaceModulation oInnerMod = new SurfaceModulation(new LineModulation(fGetHotWallRadiusRatio));
                SurfaceModulation oOuterMod = new SurfaceModulation(new LineModulation(fGetChannelOuterRadiusRatio));
                oChannel.SetRadius(oInnerMod, oOuterMod);

                Mesh mshSingle = oChannel.mshConstruct();
                AppendMesh(ref mshAllChannels, mshSingle);
            }

            return mshAllChannels;
        }

        public Voxels voxConstructChannels()
        {
            Mesh msh = mshConstructChannels();
            return new Voxels(msh);
        }

        // --- Modulation Callbacks ---

        protected float fGetHotWallRadiusRatio(float fLengthRatio)
        {
            return m_oContour.fGetHotWallRadiusAtRatio(fLengthRatio);
        }

        protected float fGetChannelOuterRadiusRatio(float fLengthRatio)
        {
            float fHeight = m_oAeroThermal.fGetChannelHeightMM(fLengthRatio);
            return m_oContour.fGetChannelOuterRadiusAtRatio(fLengthRatio, fHeight);
        }

        protected float fGetChannelPhiRange(float fLengthRatio)
        {
            float fRadius = m_oContour.fGetHotWallRadiusAtRatio(fLengthRatio);
            return m_oAeroThermal.fGetChannelPhiWidthRad(fRadius);
        }

        /// <summary>
        /// Appends triangles of source mesh into target mesh.
        /// </summary>
        public static void AppendMesh(ref Mesh target, Mesh source)
        {
            int nTriangles = source.nTriangleCount();
            for (int i = 0; i < nTriangles; i++)
            {
                Triangle tri = source.oTriangleAt(i);
                target.nAddTriangle(
                    source.vecVertexAt(tri.A),
                    source.vecVertexAt(tri.B),
                    source.vecVertexAt(tri.C)
                );
            }
        }
    }
}
