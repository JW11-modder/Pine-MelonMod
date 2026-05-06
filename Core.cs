using HarmonyLib;
using JModder;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;
using UnityEngine.EventSystems;
using static AssetBundles;
using static Human;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

[assembly: MelonInfo(typeof(PineMelonMod.Core), "jw11-modder.PineMelonMod", "1.0.0", "jw11-modder", null)]
[assembly: MelonGame("Twirlbound", "Pine")]

namespace PineMelonMod
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }

        public static LegacyInput InputHandler;

        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;

        private static MelonPreferences_Entry<bool> configPlayerNoDamageToggle;
        private static MelonPreferences_Entry<bool> configPlayerNoSpellCostToggle;
        private static MelonPreferences_Entry<bool> configPlayerUnlimitedAmmoToggle;
        private static MelonPreferences_Entry<bool> configInvMaxStacksToggle;
        private static MelonPreferences_Entry<bool> configFreeCraftToggle;

        private static MelonPreferences_Entry<float> configPlayerDamageMultiplier;
        //private static MelonPreferences_Entry<float> configPlayerXPMultiplier;
        //private static MelonPreferences_Entry<float> configPlayerSpeedMultiplier;
        //private static MelonPreferences_Entry<float> configPlayerJumpMultiplier;

        private static MelonPreferences_Entry<int> configPlayerInvRowMultiplier;

        private static MelonPreferences_Entry<KeyCode> configMenuToggle;
        //private static Key configMenuToggleKey;

        private static bool speedSet = false;
        private static bool jumpSet = false;
        public static EventSystem EventSys { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;

            MultiplierFloatCategory = MelonPreferences.CreateCategory("FloatMultipliers");
            MultiplierIntCategory = MelonPreferences.CreateCategory("IntMultipliers");
            ToggleCategory = MelonPreferences.CreateCategory("Toggles");

            configPlayerNoDamageToggle = ToggleCategory.CreateEntry<bool>("configPlayerNoDamageToggle", false, "Disable taking damage");
            configPlayerNoSpellCostToggle = ToggleCategory.CreateEntry<bool>("configPlayerNoSpellCostToggle", false, "Disable energy loss");
            configPlayerUnlimitedAmmoToggle = ToggleCategory.CreateEntry<bool>("configPlayerUnlimitedAmmoToggle", false, "Disable ammo cost");
            configInvMaxStacksToggle = ToggleCategory.CreateEntry<bool>("configInvMaxStacksToggle", false, "Set item stack limit to 9999");
            configFreeCraftToggle = ToggleCategory.CreateEntry<bool>("configFreeCraftToggle", false, "Can craft without ingredients");

            configPlayerDamageMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerDamageMultiplier", 1f, "Player damage to enemies multiplier", validator: new ValueRange<float>(1f, 20f));
            //configPlayerXPMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerXPMultiplier", 1f, "Player XP multiplier", validator: new ValueRange<float>(1f, 20f));
            //configPlayerSpeedMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerSpeedMultiplier", 1f, "Player speed multiplier", validator: new ValueRange<float>(1f, 20f));
            //configPlayerJumpMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerJumpMultiplier", 1f, "Player jump height multiplier", validator: new ValueRange<float>(1f, 20f));

            configPlayerInvRowMultiplier = MultiplierIntCategory.CreateEntry<int>("configPlayerInvRowMultiplier", 2, "Player inventory rows count (without upgrades)", validator: new ValueRange<int>(2, 60));

            HarmonyInstance.PatchAll();

            JMod.Init(Instance);
            configMenuToggle = JMod.configMenuToggle;

            JMod.Log("Pine Mod Initialized.");
        }
        public override void OnUpdate()
        {
            
            if (Input.GetKeyDown(configMenuToggle.Value))
            {
                //ControlManager.InputManager
                if (JMod.SwitchMenu())
                {
                    JMod.Log("Show mod menu!");
                }
                else
                {
                    JMod.Log("Hide mod menu!");
                }
            }
        }

        public override void OnGUI()
        {
            JMod.ShowMenu();
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.HasEnough))]
        static class HasEnoughToCraftPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeCraftToggle.Value)
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.Craft))]
        static class CraftPatch1
        {
            static bool Prefix(ref ItemStack.Handle _ideaHandle, ref InventoryBase _inventory, ref bool __result, ref ItemManager __instance)
            {
                if (!configFreeCraftToggle.Value)
                {
                    return true;
                }
                ItemInfo itemInfo;
                _ideaHandle.TryGetItemInfo(out itemInfo);
                ItemInfo_Idea itemInfo_Idea = (ItemInfo_Idea)itemInfo;

                Type ISCreator = AccessTools.Inner(typeof(ItemManager), "ItemStackCreator");


                ItemStackCreator itemStackCreator = new ItemStackCreator();

                ItemStack.Handle itemHandle2 = ItemStack.Handle.GetItemHandle(new ItemStack(itemStackCreator, itemInfo_Idea.GetItemToCraftID(), itemInfo_Idea.GetItemToCraftQuantity()));
                _inventory.TakeFromStack(itemHandle2, itemHandle2.GetQuantity(), null);
                ItemStack.Handle.Destroy(ref itemHandle2);
                __result = true;
                return false;
            }
        }
        private class ItemStackCreator : ItemManager.IItemStackCreator
        {
        }


        [HarmonyPatch(typeof(InventoryLimited), nameof(InventoryLimited.GetStackLimit))]
        static class InventoryRowsPatch1
        {
            static bool Prefix(ref int __result, InventoryLimited __instance)
            {
                if (configPlayerInvRowMultiplier.Value <= 1)
                {
                    return true;
                }
                if (__instance.HasItem(ItemID.Special.inventory_upgrade_02))
                {
                    __result = 24 * configPlayerInvRowMultiplier.Value;
                    return false;
                }
                if (__instance.HasItem(ItemID.Special.inventory_upgrade_01))
                {
                    __result = 16 * configPlayerInvRowMultiplier.Value;
                    return false;
                }
                __result = 8 * configPlayerInvRowMultiplier.Value;
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.TryGetMaxStack))]
        static class StackLimitPatch5
        {
            static bool Prefix(out int _maxStack, ItemManager __instance, ref bool __result)
            {
                if (!configInvMaxStacksToggle.Value)
                {
                    _maxStack = 0;
                    return true;
                }
                if (__instance.itemCreationDataLoaded)
                {
                    _maxStack = 9999;
                    __result = true;
                }
                else
                {
                    _maxStack = 0;
                    __result = false;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemManager), "LoadItemCreationData")]
        static class StackLimitPatch1
        {
            static void Postfix(ItemManager __instance, ref Dictionary<ItemID, ItemCreationData.ItemData.ItemDefinition> ___idToDefinition)
            {
                if (!configInvMaxStacksToggle.Value)
                {
                    return;
                }
                Dictionary<ItemID, ItemCreationData.ItemData.ItemDefinition>  tempDict = ___idToDefinition;

                foreach (KeyValuePair<ItemID, ItemCreationData.ItemData.ItemDefinition> item in tempDict)
                {
                    if (!item.Value.isQuestItem)
                        item.Value.maxStack = 9999;
                }
                ___idToDefinition = tempDict;
            }
        }

        //ItemCreationData
        [HarmonyPatch(typeof(ItemCreationData), nameof(ItemCreationData.AddItemDefinition))]
        static class StackLimitPatch2
        {
            static bool Prefix(ItemCreationData __instance, ref int _maxStack)
            {
                if (!configInvMaxStacksToggle.Value)
                {
                    return true;
                }
                if (_maxStack > 1)
                {
                    _maxStack = 9999;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(ItemStackLimited), "GetAmountUntilFull")]
        static class StackLimitPatch3
        {
            static bool Prefix(ItemStackLimited __instance, ref int ___maxStack, int ___quantity, ref int __result)
            {
                if (!configInvMaxStacksToggle.Value)
                {
                    return true;
                }
                ___maxStack = 9999;
                __result = ___maxStack - ___quantity;
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemStackLimited), "IsFull")]
        static class StackLimitPatch4
        {
            static bool Prefix(ItemStackLimited __instance, ref int ___maxStack, int ___quantity, ref bool __result)
            {
                if (!configInvMaxStacksToggle.Value)
                {
                    return true;
                }
                ___maxStack = 9999;
                __result = (___maxStack == ___quantity);
                return false;
            }
        }

        [HarmonyPatch(typeof(Equipment.Tools), nameof(Equipment.Tools.GetAmmoCount))]
        static class GetAmmoPatch1
        {
            static bool Prefix(Equipment.Tools __instance, ref int __result)
            {
                if (!configPlayerUnlimitedAmmoToggle.Value)
                {
                    return true;
                }
                __result = 999;
                return false;
            }
        }

        [HarmonyPatch(typeof(Equipment.Tools), nameof(Equipment.Tools.ConsumeAmmo))]
        static class ConsumeAmmoPatch1
        {
            static bool Prefix(Equipment.Tools __instance, Organism ___organism)
            {
                if (!configPlayerUnlimitedAmmoToggle.Value)
                    return true;
                if (___organism == Singleton<GameLogic>.instance.player)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Equipment.Tools.ToolSet), nameof(Equipment.Tools.ToolSet.Equip))]
        static class EquipToolPatch1
        {
            static bool Prefix(ref Equipment.Tools.ToolSet __instance, ref ItemInfo_Tool _itemInfo, Organism _organism)
            {
                if (configPlayerDamageMultiplier.Value <= 1)
                    return true;
                if (_organism == Singleton<GameLogic>.instance.player)
                {
                    if (_itemInfo.type == ItemInfo_Tool.ToolType.SWORD)
                    {
                        ItemInfo_Sword weapon = (ItemInfo_Sword)_itemInfo;
                        weapon.damageMultiplier *= configPlayerDamageMultiplier.Value;
                        _itemInfo = weapon;
                    }
                    if (_itemInfo.type == ItemInfo_Tool.ToolType.BOW) 
                    {
                        ItemInfo_Bow weapon = (ItemInfo_Bow)_itemInfo;
                        weapon.damageMultiplier *= configPlayerDamageMultiplier.Value;
                        _itemInfo = weapon;
                    }
                    if (_itemInfo.type == ItemInfo_Tool.ToolType.SLINGSHOT)
                    {
                        ItemInfo_Slingshot weapon = (ItemInfo_Slingshot)_itemInfo;
                        weapon.damageMultiplier *= configPlayerDamageMultiplier.Value;
                        _itemInfo = weapon;
                    }
                }
                return true;
            }
        }

        //((Equipment.Tools.IToolUsingBrain)this.organism.brain).tools
        //Equipment.Tools
        //Singleton<GameLogic>.instance.player !!
        //GameLogic.SetPlayer - Postfix __instance.player
        //configPlayerNoDamageToggle
        //configPlayerNoSpellCostToggle

        [HarmonyPatch(typeof(GameLogic), "Update")]
        static class PlayerHealthPatch1
        {
            static bool Prefix(ref Organism _____player)
            {
                if (configPlayerNoDamageToggle.Value)
                {
                    _____player.combatInfo.SetIsInvincible(true);
                    float maxHealth = _____player.stats.GetMaxHealth();
                    float currentHealth = _____player.stats.GetHealth();
                    if (currentHealth < maxHealth)
                        _____player.stats.AddHealth(maxHealth - currentHealth);
                }
                if (configPlayerNoSpellCostToggle.Value)
                {
                    _____player.stats.AddFullness(1f);
                    _____player.stats.AddSocial(1f);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Human.Energy), "ConsumeEnergy")]
        static class PlayerEnergyPatch1
        {
            static bool Prefix(ref Organism ___organism)
            {

                if (!configPlayerNoSpellCostToggle.Value)
                    return true;
                if (___organism == Singleton<GameLogic>.instance.player)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Human.Energy), "DoEnergyChecking")]
        static class PlayerEnergyPatch2
        {
            static bool Prefix(ref Organism ___organism)
            {

                if (!configPlayerNoSpellCostToggle.Value)
                    return true;
                if (___organism == Singleton<GameLogic>.instance.player)
                {
                    return false;
                }

                return true;
            }
        }



        //Human.StatusModifiers
        //configPlayerSpeedMultiplier
        //configPlayerJumpMultiplier
        /*[HarmonyPatch(typeof(Human.Stats_0), nameof(Human.Stats_0.Update))]
        static class PlayerStatusPatch1
        {
            static bool Prefix(ref Human.Stats_0 __instance)
            {
                if (configPlayerSpeedMultiplier.Value <= 1f && configPlayerJumpMultiplier.Value <= 1f)
                    return true;
                else
                {
                    if (__instance.organism == Singleton<GameLogic>.instance.player)
                    {
                        float speed = __instance.statusModifiers.GetAttribute(Human.StatusModifiers.Attribute.MOVEMENT_SPEED);
                        JMod.Log("Current maximum speed is: " + speed.ToString());
                        Human.StatusModifiers modifiers = __instance.statusModifiers;
                        //Human.StatusModifiers.ModifiedAttribute[] attributes;
                        HumanCreationData.HumanProperties.
                        ModifiedAttribute[] attributes

                        stats_.statusModifiers.ApplyStatusModifier(speedModifier);
                        stats_.statusModifiers.SetAttribute(Human.StatusModifiers.Attribute.MOVEMENT_SPEED);
                        float jump = stats_.statusModifiers.GetAttribute(Human.StatusModifiers.Attribute.JUMP_HEIGHT);
                        JMod.Log("Current maximum jump height is: " + jump.ToString());
                        __instance.statusModifiers = ;
                    }
                }

                return true;
            }
        }*/



    }


}