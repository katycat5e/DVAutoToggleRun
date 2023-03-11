using System;
using System.Collections.Generic;
using System.Linq;
using UnityModManagerNet;

namespace DVAutoToggleRun
{
    public static class AutoToggleRunMain
    {
        public static UnityModManager.ModEntry ModEntry;
        public static ATR_Settings Settings;
        private static bool wasWalking = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            modEntry.OnToggle += OnToggle;

            // Load preferences
            Settings = UnityModManager.ModSettings.Load<ATR_Settings>(ModEntry);
            ModEntry.OnGUI = DrawGUI;
            ModEntry.OnSaveGUI = SaveGUI;

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool enable)
        {
            if (enable)
            {
                WorldStreamingInit.LoadingFinished += OnWorldLoaded;
                PlayerManager.CarChanged += OnPlayerCarChanged;

                if (WorldStreamingInit.IsLoaded)
                {
                    OnWorldLoaded();
                }
            }
            else
            {
                WorldStreamingInit.LoadingFinished -= OnWorldLoaded;
                PlayerManager.CarChanged -= OnPlayerCarChanged;
            }

            return true;
        }

        private static void OnWorldLoaded()
        {
            ModEntry.Logger.Log("World loaded, initializing autorun");
            OnPlayerCarChanged(PlayerManager.Car);
        }

        public static void OnPlayerCarChanged(TrainCar newCar)
        {
            bool shouldWalk = (newCar != null) && 
                (Settings.WagonsPreferWalk ||
                CarTypes.IsAnyLocomotiveOrTender(newCar.carType) ||
                CarTypes.IsCaboose(newCar.carType));

            if (shouldWalk != wasWalking)
            {
                GamePreferences.Set(Preferences.AlwaysRunToggle, !shouldWalk);
                ModEntry.Logger.Log($"Set autorun to {shouldWalk}");
                wasWalking = shouldWalk;
            }
        }

        static void DrawGUI(UnityModManager.ModEntry entry)
        {
            Settings.Draw(entry);
        }

        static void SaveGUI(UnityModManager.ModEntry entry)
        {
            Settings.Save(entry);
        }
    }

    public class ATR_Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Prefer walking on rolling stock")]
        public bool WagonsPreferWalk = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            AutoToggleRunMain.OnPlayerCarChanged(PlayerManager.Car);
        }
    }
}
