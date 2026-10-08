# Dizzy Cheats Like a Pirate

Sailwind BepInEx 5 plugin with cheats for testing other mods: fly, super speed, god mode, a panel to set your needs, and an invisible motor for your boat.

Every cheat starts off when the game launches, except god mode, which remembers its last setting.

```powershell
dotnet build src\Dizzy.Cheats\Dizzy.Cheats.csproj -c Release
```

Deploy the DLL into your ModPack with the sailwind-modpack-deploy skill, not into the game folder.

## Keys

| Key | Does |
|-----|------|
| `` ` `` | Open the cheat panel (switches, motor status, needs sliders, gold) |
| `V` | Fly on / off |
| `B` | Super speed on / off (walk, run, swim and fly x4) |
| `H` | God mode on / off (turning it on with this key also fills every need) |
| `I` / `K` | Motor: throttle up / down, one step per press |
| `J` / `L` | Motor: hold to turn left / right |
| `O` | Motor: all stop |

All keys can be changed in the config. Cheat keys do nothing in menus or shops.

The panel only opens during play. In the Esc menu, `` ` `` belongs to Sailwind Console if you have it, which only opens there. Esc closes the panel and nothing else, the same way it closes the game's shop window; press Esc again for the Esc menu.

### Fly

Two styles, set by `[Fly] Style`:

- **Hover** (default): move where you look. Your jump key (Space) rises, your crouch key (Ctrl) sinks, and you stay in the air when you let go. Hold run (Shift) to go faster. `NoClip = true` lets you fly through hulls, walls and the ground.
- **Flappy**: hold jump to rise, let go to fall. You land on whatever is under you, so no-clip is ignored.

Flying does not use food or water. Crouch does not toggle while you fly.

While you are in the air, your weight (and anything you hold) is taken off your boat. Otherwise hovering above the deck acts like a weight on a tall pole and rolls the boat over. Once you stand on something again, including a Flappy landing on the deck, you weigh the boat down as normal.

### Super speed

Multiplies your normal walk, run and swim speed (`[Speed] Multiplier`, default 4). Moving faster does not cost extra food or water.

It also multiplies fly speed, on top of the Shift boost: with the defaults, flying with super speed and Shift is 8 × 4 × 4 = 128 m/s. In Flappy style, the jump key's rise and the fall speed stay the same.

### God mode and needs

God mode uses the game's own god mode flag: food, water, sleep, vitamins and protein stop changing. You can still eat and drink while it is on.

The panel has a slider for each need. With god mode on, a need stays where you set it. With god mode off, it drains from there as normal. Raising a need also clears any debt on it, so it does not drain straight back down.

### Gold

The panel has a row for each of the four currencies: Al'Ankh Lions, Emerald Dragons, Aestrin Crowns and Gold Lions. Pick an amount (100, 1,000, 10,000 or 100,000), then press **Add** or **Remove** on a row. Remove stops at 0.

### Invisible motor

Works on the boat you are standing on. Each throttle step pushes the boat at `[Motor] StepAcceleration` m/s² (default 1). There are 20 steps ahead and 3 astern. The push ignores boat size, so all boats speed up the same; water drag still limits top speed. Turning swings the bow without the rudder.

The motor stops by itself when you step off the boat, so a boat cannot sail away without you.

Config: `BepInEx\config\com.dizzy.sailwind.cheats.cfg`

## Install

Requires Sailwind + [BepInEx 5](https://thunderstore.io/c/sailwind/p/BepInEx/BepInExPack/) (Thunderstore BepInExPack recommended). Extract `Dizzy.Cheats` into `BepInEx\plugins\`. Restart Sailwind and check `BepInEx\LogOutput.log`.

## License

See [LICENSE](LICENSE).
