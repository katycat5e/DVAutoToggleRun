using System;
using System.Collections.Generic;
using System.Linq;
using UnityModManagerNet;

namespace DVAutoToggleRun
{
    public static class AutoToggleRunMain
    {
        public static UnityModManager.ModEntry ModEntry;
        private static ATR_Settings Settings;
        private static bool wasOnWalkCar = false;

        public static bool Load( UnityModManager.ModEntry modEntry )
        {
            ModEntry = modEntry;

            WorldStreamingInit.LoadingFinished += OnWorldLoaded;
            PlayerManager.CarChanged += OnPlayerCarChanged;

            // Load preferences
            Settings = UnityModManager.ModSettings.Load<ATR_Settings>(ModEntry);
            ModEntry.OnGUI = DrawGUI;
            ModEntry.OnSaveGUI = SaveGUI;

            return true;
        }

        private static void OnWorldLoaded()
        {
            ModEntry.Logger.Log("World loaded, initializing autorun");
            OnPlayerCarChanged(PlayerManager.Car);
        }

        internal static void OnPlayerCarChanged( TrainCar newCar )
        {
            bool onWalkCar = (newCar != null) && (Settings.WagonsPreferWalk || CarTypes.IsAnyLocomotiveOrTender(newCar.carType));

            if( onWalkCar ^ wasOnWalkCar )
            {
                GamePreferences.Set(Preferences.AlwaysRunToggle, !onWalkCar);
                ModEntry.Logger.Log($"Set autorun to {onWalkCar}");
                wasOnWalkCar = onWalkCar;
            }
        }

        static void DrawGUI( UnityModManager.ModEntry entry )
        {
            Settings.Draw(entry);
        }

        static void SaveGUI( UnityModManager.ModEntry entry )
        {
            Settings.Save(entry);
        }
    }

    public class ATR_Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Prefer walking on rolling stock")]
        public bool WagonsPreferWalk = false;

        public override void Save( UnityModManager.ModEntry modEntry )
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            AutoToggleRunMain.OnPlayerCarChanged(PlayerManager.Car);
        }
    }
}
