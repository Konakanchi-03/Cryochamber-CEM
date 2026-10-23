using System;

namespace CryoChamber.Configuration
{
    /// <summary>
    /// Holds all thermodynamic, geometric, and additive manufacturing (DfAM) parameters
    /// for the regenerative cryogenic thrust chamber.
    /// Follows LEAP 71 Hungarian notation conventions:
    /// f = float, d = double, i = int, n = uint, b = bool.
    /// </summary>
    public class ChamberParameters
    {
        // --- Thermodynamic & Performance Specs ---
        public float m_fChamberPressureBar      { get; set; } = 35.0f;     // Pc = 35 bar
        public float m_fThrustNewton            { get; set; } = 7500.0f;   // 7.5 kN nominal thrust
        public float m_fMassFlowRateKgS         { get; set; } = 2.4f;      // Total mass flow rate
        public float m_fCoolantMassFlowKgS      { get; set; } = 0.40f;     // LH2 coolant flow (~1/6 O/F ratio)
        public float m_fCoolantInletTempK       { get; set; } = 25.0f;     // Subcooled cryogenic LH2
        public float m_fGasRecoveryTempK        { get; set; } = 3200.0f;   // LOX/LH2 combustion temperature

        // --- Core Chamber & Nozzle Geometry (mm) ---
        public float m_fThroatRadiusMM          { get; set; } = 18.0f;     // Rt = 18 mm (Dt = 36 mm)
        public float m_fChamberRadiusMM         { get; set; } = 36.0f;     // Rc = 36 mm (Contraction ratio = 4.0)
        public float m_fExitRadiusMM            { get; set; } = 54.0f;     // Re = 54 mm (Expansion ratio = 9.0)
        
        public float m_fChamberLengthMM         { get; set; } = 65.0f;     // Length of cylindrical combustion chamber
        public float m_fConvergentLengthMM      { get; set; } = 30.0f;     // Convergent cone section
        public float m_fNozzleLengthMM          { get; set; } = 85.0f;     // Supersonic expansion bell
        public float m_fTotalLengthMM           => m_fChamberLengthMM + m_fConvergentLengthMM + m_fNozzleLengthMM; // 180 mm

        // --- Regenerative Cooling Channels Architecture ---
        public uint  m_nChannelCount            { get; set; } = 64;        // 64 conformal cooling channels
        public float m_fHotWallThicknessMM      { get; set; } = 1.2f;      // Minimum inner liner thickness (conduction)
        public float m_fJacketThicknessMM       { get; set; } = 2.0f;      // Outer structural containment wall
        public float m_fMinRibThicknessMM       { get; set; } = 0.8f;      // Minimum rib width for LPBF printing

        // --- Variable Channel Aspect Ratio (Throat vs Chamber vs Exit) ---
        public float m_fChannelHeightThroatMM   { get; set; } = 3.8f;      // Tall channel at throat for high surface area
        public float m_fChannelHeightChamberMM  { get; set; } = 2.5f;      // Moderate height in chamber
        public float m_fChannelHeightExitMM     { get; set; } = 1.4f;      // Low profile at skirt to reduce pressure drop

        // --- Manifolds & Flanges (mm) ---
        public float m_fManifoldMajorRadiusMM   { get; set; } = 10.0f;     // Torus profile radius
        public float m_fManifoldWallMM          { get; set; } = 2.0f;      // Manifold pressure vessel wall
        public float m_fInletPipeDiameterMM     { get; set; } = 14.0f;     // LH2 feed line
        public float m_fOutletPipeDiameterMM    { get; set; } = 14.0f;     // Warm H2 delivery to injector

        // --- Resolution & Visualization ---
        public float m_fVoxelSizeMM             { get; set; } = 0.5f;      // Default interactive voxel preview
        public float m_fExportVoxelSizeMM       { get; set; } = 0.2f;      // High-resolution for manufacturing export
    }
}
