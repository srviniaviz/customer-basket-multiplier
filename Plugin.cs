using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace CustomerBasket2x;

[BepInPlugin("vinicius.customerbasket2x", "Customer Basket Multiplier", "1.2.0")]
public sealed class Plugin : BasePlugin
{
    private static ManualLogSource _log;
    private static int _multiplier;
    private static float _showUntil;
    private static readonly Dictionary<IntPtr, (int types, int items)> Originals = new();
    private static bool _sawShoppingList;

    public override void Load()
    {
        _log = Log;
        _multiplier = Config.Bind("Shopping", "DefaultMultiplier", 2,
            "Initial customer shopping multiplier. Choose 1, 2, 3, or 5.").Value;
        if (_multiplier != 1 && _multiplier != 2 && _multiplier != 3 && _multiplier != 5)
            _multiplier = 2;
        Harmony.CreateAndPatchAll(typeof(StrategyPatch), "vinicius.customerbasket2x");
        Harmony.CreateAndPatchAll(typeof(ShoppingListPatch), "vinicius.customerbasket2x.list");
        ClassInjector.RegisterTypeInIl2Cpp<HotkeyOverlay>();
        var go = new GameObject("Customer Basket Hotkeys");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<HotkeyOverlay>();
        _showUntil = Time.unscaledTime + 5f;
        Log.LogInfo($"Loaded; customer shopping x{_multiplier}. Hotkeys: Ctrl+Alt+1/2/3/4.");
    }

    private static void IncreaseLimits(CustomerStrategySO strategy)
    {
        if (strategy == null || strategy.Pointer == IntPtr.Zero) return;
        try
        {
            var key = strategy.Pointer;
            if (!Originals.TryGetValue(key, out var original))
            {
                original = (strategy.MaxProductVariantsCountToBuy, strategy.MaxProductCountToBuy);
                Originals[key] = original;
            }
            var types = (int)Math.Min(1000L, (long)original.types * _multiplier);
            var items = (int)Math.Min(1000L, (long)original.items * _multiplier);
            var changed = strategy.MaxProductVariantsCountToBuy != types ||
                          strategy.MaxProductCountToBuy != items;
            if (strategy.MaxProductVariantsCountToBuy != types)
                strategy.MaxProductVariantsCountToBuy = types;
            if (strategy.MaxProductCountToBuy != items)
                strategy.MaxProductCountToBuy = items;
            if (changed)
                _log?.LogInfo($"Customer strategy limits: types {original.types}->{types}, items {original.items}->{items}.");
        }
        catch (Exception ex)
        {
            _log?.LogError($"Could not update customer strategy: {ex}");
        }
    }

    private static void SelectMultiplier(int multiplier)
    {
        if (_multiplier == multiplier) return;
        _multiplier = multiplier;
        _showUntil = Time.unscaledTime + 5f;
        _log?.LogInfo($"Customer shopping multiplier changed to x{multiplier}; applies to new shopping lists.");
    }

    public sealed class HotkeyOverlay : MonoBehaviour
    {
        public HotkeyOverlay(IntPtr pointer) : base(pointer) { }

        private void Update()
        {
            if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ||
                !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMultiplier(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectMultiplier(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectMultiplier(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectMultiplier(5);
        }

        private void OnGUI()
        {
            if (Time.unscaledTime > _showUntil) return;
            var label = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "pt"
                ? $"Compras dos clientes: {_multiplier}x"
                : $"Customer shopping: {_multiplier}x";
            GUI.Box(new Rect(Screen.width - 230, 70, 215, 34), label);
        }
    }

    [HarmonyPatch(typeof(CustomerStrategiesManager), "get_CurrentStrategy")]
    private static class StrategyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CustomerStrategySO __result) => IncreaseLimits(__result);
    }

    [HarmonyPatch(typeof(CustomerManager), "CreateShoppingList")]
    private static class ShoppingListPatch
    {
        [HarmonyPrefix]
        private static void Prefix(CustomerManager __instance)
        {
            try
            {
                var strategy = __instance.m_CurrentStrategy;
                if (!_sawShoppingList)
                {
                    _sawShoppingList = true;
                    _log?.LogInfo($"First shopping list; strategy available={strategy != null}.");
                }
                IncreaseLimits(strategy);
            }
            catch (Exception ex)
            {
                _log?.LogError($"Could not inspect shopping list strategy: {ex}");
            }
        }
    }
}
