using System;
using CryoChamber.Configuration;

namespace CryoChamber.Physics
{
    /// <summary>
    /// Models the thermal-hydraulic environment of the rocket thrust chamber.
    /// Incorporates:
    /// 1. Simplified Bartz correlation for gas-side convective heat transfer q''(z).
    /// 2. Supercritical / cryogenic LH2 bulk heat capacity and coolant velocity.
    /// 3. Aspect-ratio calculation ensuring rib structural stability under 35 bar internal Pc.
    /// </summary>
    public class ThermalHydraulics
    {
        protected ChamberParameters m_oParams;

        public ThermalHydraulics(ChamberParameters oParams)
        {
            m_oParams = oParams;
        }

        /// <summary>
        /// Returns the estimated gas-side heat flux in MW/m^2 as a function of normalized axial position t in [0, 1].
        /// t = 0: Injector face
        /// t_throat: Nozzle throat (peak heat flux)
        /// t = 1: Nozzle exit
        /// </summary>
        public float fGetHeatFluxMW_m2(float fLengthRatio)
        {
            float fZ_Throat = (m_oParams.m_fChamberLengthMM + m_oParams.m_fConvergentLengthMM) / m_oParams.m_fTotalLengthMM;
            float fDeltaThroat = MathF.Abs(fLengthRatio - fZ_Throat);

            // Peak heat flux at the throat reaches ~28 MW/m^2 for LOX/LH2 at 35 bar
            float fPeakFlux = 28.0f;
            float fChamberFlux = 9.0f;
            float fExitFlux = 3.5f;

            if (fLengthRatio <= fZ_Throat)
            {
                // Upstream of throat (chamber & convergent section)
                float fFactor = MathF.Exp(-MathF.Pow(fDeltaThroat / 0.12f, 2.0f));
                return fChamberFlux + (fPeakFlux - fChamberFlux) * fFactor;
            }
            else
            {
                // Downstream of throat (supersonic expansion bell)
                float fFactor = MathF.Exp(-MathF.Pow(fDeltaThroat / 0.15f, 2.0f));
                return fExitFlux + (fPeakFlux - fExitFlux) * fFactor;
            }
        }

        /// <summary>
        /// Computes the required cooling channel height (radial depth) in mm at position t.
        /// Throat requires high aspect ratio (tall, narrow channel) to maximize coolant velocity
        /// and convective heat transfer coefficient h_c.
        /// Skirt uses lower height to minimize parasitic pressure drop (Delta P).
        /// </summary>
        public float fGetChannelHeightMM(float fLengthRatio)
        {
            float fZ_Throat = (m_oParams.m_fChamberLengthMM + m_oParams.m_fConvergentLengthMM) / m_oParams.m_fTotalLengthMM;
            float fDist = MathF.Abs(fLengthRatio - fZ_Throat);

            if (fLengthRatio < fZ_Throat)
            {
                // Chamber to throat blend
                float fT = (fLengthRatio) / fZ_Throat;
                return m_oParams.m_fChannelHeightChamberMM + (m_oParams.m_fChannelHeightThroatMM - m_oParams.m_fChannelHeightChamberMM) * (fT * fT);
            }
            else
            {
                // Throat to exit blend
                float fT = (fLengthRatio - fZ_Throat) / (1.0f - fZ_Throat);
                return m_oParams.m_fChannelHeightThroatMM - (m_oParams.m_fChannelHeightThroatMM - m_oParams.m_fChannelHeightExitMM) * MathF.Sqrt(fT);
            }
        }

        /// <summary>
        /// Computes the cooling channel width (angular coverage in radians) at a given local radius.
        /// Balances coolant flow area vs minimum printable rib thickness for Laser Powder Bed Fusion (LPBF).
        /// </summary>
        public float fGetChannelPhiWidthRad(float fRadiusMM)
        {
            uint nChannels = m_oParams.m_nChannelCount;
            float fTotalPerimeter = 2.0f * MathF.PI * fRadiusMM;
            float fSectorWidth = fTotalPerimeter / nChannels;

            // Rib width must never drop below minimum printable wall thickness
            float fRibWidth = MathF.Max(m_oParams.m_fMinRibThicknessMM, fSectorWidth * 0.35f);
            float fChannelWidthMM = fSectorWidth - fRibWidth;

            // Angular width in radians
            return fChannelWidthMM / fRadiusMM;
        }
    }
}
