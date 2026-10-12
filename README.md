# Customer Basket Multiplier

A BepInEx 6 IL2CPP mod for **Supermarket Simulator** that multiplies the game's limits for different product types and quantity per product in newly generated customer shopping lists.

The default is 2×. The mod changes limits, so actual baskets may be smaller depending on the game's selection rules, unlocked products, and stock.

## Requirements

- Supermarket Simulator for Windows. Tested locally on the September 2026 game build (patch 1.7.1).
- [Tobey's BepInEx Pack for Supermarket Simulator](https://www.nexusmods.com/supermarketsimulator/mods/9), BepInEx 6 IL2CPP.

Single-player has been tested. Multiplayer behavior has not been tested; the host should install the mod because the host generates customer shopping lists.

## Installation

1. Close the game.
2. Install Tobey's BepInEx Pack and launch the game once.
3. Extract this ZIP into the `Supermarket Simulator` game directory. The DLL should end up at `BepInEx/plugins/CustomerBasket2x/CustomerBasket2x.dll`.
4. Start the game. The default multiplier is 2×.

No save editing is needed. Existing customer shopping lists are not changed.

## Hotkeys

| Keys | Shopping multiplier |
| --- | ---: |
| Ctrl+Alt+1 | 1× (original limits) |
| Ctrl+Alt+2 | 2× |
| Ctrl+Alt+3 | 3× |
| Ctrl+Alt+4 | 5× |

A small on-screen notice appears for five seconds after switching. The chosen value applies to shopping lists generated after the change. On restart, the initial value comes from `BepInEx/config/vinicius.customerbasket2x.cfg` (`DefaultMultiplier`, initially 2).

## Compatibility and removal

The mod patches `CustomerStrategiesManager.CurrentStrategy` and `CustomerManager.CreateShoppingList`, and changes `MaxProductVariantsCountToBuy` and `MaxProductCountToBuy` in memory. It does not modify save files. Other mods that replace customer shopping-list logic may conflict.

To remove it, close the game and delete `BepInEx/plugins/CustomerBasket2x/CustomerBasket2x.dll`.

## Source and credit

Source is available in this repository and in the separate source archive under the MIT License. Created for srvinicius0000 with Codex assistance. The mod code and this description were generated with AI and then compiled and tested locally.

## Build from source

Install the .NET SDK and Tobey's BepInEx Pack, then run `dotnet build -c Release -p:GameDir="C:\\path\\to\\Supermarket Simulator"`. The project references game and BepInEx assemblies from that directory; they are not bundled with the source. The compiled DLL is in `bin/Release/net6.0/`.
