using System;
using CryoChamber.Configuration;

namespace CryoChamber.Geometry
{
    /// <summary>
    /// Computes the radial inner contour of the thrust chamber and nozzle.
    /// Incorporates:
    /// - Cylindrical chamber section (injector face to convergent inlet)
    /// - Smooth contraction cone with throat circular blend
    /// - Supersonic expansion bell (parabolic approximation of a Rao 80% contour)
    /// </summary>
    public class NozzleContour
    {
        protected ChamberParameters m_oParams;

        public NozzleContour(ChamberParameters oParams)
        {
            m_oParams = oParams;
        }

        /// <summary>
        /// Returns the inner gas-side radius (mm) as a function of axial position z (mm).
        /// z = 0 : Injector face
        /// z = z_throat : Throat minimum
        /// z = z_total : Nozzle skirt exit
        /// </summary>
        public float fGetInnerRadiusAtZ(float fZ)
        {
            float fZ_Cyl = m_oParams.m_fChamberLengthMM;
            float fZ_Conv = fZ_Cyl + m_oParams.m_fConvergentLengthMM;
            float fZ_Total = m_oParams.m_fTotalLengthMM;

            float fRc = m_oParams.m_fChamberRadiusMM;
            float fRt = m_oParams.m_fThroatRadiusMM;
            float fRe = m_oParams.m_fExitRadiusMM;

            if (fZ <= fZ_Cyl)
            {
                // 1. Cylindrical combustion section
                return fRc;
            }
            else if (fZ <= fZ_Conv)
            {
                // 2. Convergent section with smooth cubic hermite transition to throat
                float fT = (fZ - fZ_Cyl) / (fZ_Conv - fZ_Cyl);
                // Smooth S-curve (cosine or cubic smoothstep)
                float fSmooth = 0.5f * (1.0f + MathF.Cos(fT * MathF.PI));
                return fRt + (fRc - fRt) * fSmooth;
            }
            else
            {
                // 3. Supersonic expansion bell (Rao parabolic contour)
                float fZ_Nozzle = fZ - fZ_Conv;
                float fL_Nozzle = fZ_Total - fZ_Conv;
                float fT = Math.Clamp(fZ_Nozzle / fL_Nozzle, 0.0f, 1.0f);

                // Initial expansion angle ~30 deg tapering to ~10 deg at exit
                // Parabolic profile: r(t) = Rt + (Re - Rt) * (a*t^0.5 + (1-a)*t)
                float fWeight = 0.65f;
                float fCurve = fWeight * MathF.Sqrt(fT) + (1.0f - fWeight) * (fT * fT);
                return fRt + (fRe - fRt) * fCurve;
            }
        }

        /// <summary>
        /// Overload accepting normalized length ratio t in [0, 1].
        /// </summary>
        public float fGetInnerRadiusAtRatio(float fRatio)
        {
            float fZ = fRatio * m_oParams.m_fTotalLengthMM;
            return fGetInnerRadiusAtZ(fZ);
        }

        /// <summary>
        /// Returns the outer radius of the inner hot-gas liner (gas radius + liner wall thickness).
        /// </summary>
        public float fGetHotWallRadiusAtRatio(float fRatio)
        {
            return fGetInnerRadiusAtRatio(fRatio) + m_oParams.m_fHotWallThicknessMM;
        }

        /// <summary>
        /// Returns the outer radius of the coolant channel ceiling at axial ratio t.
        /// </summary>
        public float fGetChannelOuterRadiusAtRatio(float fRatio, float fChannelHeightMM)
        {
            return fGetHotWallRadiusAtRatio(fRatio) + fChannelHeightMM;
        }
    }
}
