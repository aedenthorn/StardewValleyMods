# AOE Tools
## Creating Tools
To designate a tool as an Area of Effect (AOE) tool, first create the tool using Content Patcher, e.g.:

```
{
    "Action": "EditData",
    "Target": "Data/Tools",
    "Entries": {
        "{{ModId}}_fireballWand": {
            "ClassName": "GenericTool",
            "Name": "Fireball Wand",
            "DisplayName": "{{i18n:fireballWand}}",
            "Description": "{{i18n:fireballWand.Desc}}",
            "Texture": "{{ModId}}/weapon",
            "SpriteIndex": 3,
            "UpgradeLevel": 2,
	        "SalePrice": 5000
        }
    }
}
```

Then add the tool to this mod's tools dictionary, e.g.:

```
{
    "Action": "EditData",
    "Target": "aedenthorn.AreaOfEffect/tools",
    "Entries": {
        "{{ModId}}_fireballWand": {
            "MaxCharges": 20,
            "RechargeItem": "768",
            "RechargeAmount": 10,
            "MaxDistance": 10,
            "RechargeSound": "cowboy_powerup",
            "Spells": [
                "Fireball"
            ],
            "ChargesColor": {
                "B": 0,
                "G": 69,
                "R": 255,
                "A": 255
            },
            "ChargesBackColor": {
                "B": 169,
                "G": 169,
                "R": 169,
                "A": 255
            },
            "AuraColor": {
                "B": 0,
                "G": 69,
                "R": 255,
                "A": 255
            },
			"AddToWizardBook": true
        }
    }
}
```

## Field Explanation

| Field | Type | Description |
|:-----:|:----:|:------------|
| MaxCharges | int | The maximum number of charges the tool can hold. |
| RechargeItem | string | The item ID used to recharge the tool. |
| RechargeAmount | int | The amount of charges restored when using the recharge item. |
| MaxDistance | int | The maximum distance the tool can target. |
| RechargeSound | string | The sound played when the tool is recharged. |
| [Spells](spells.md) | string[] | The list of spell IDs that the tool can cast. If omitted, all spells are allowed. If the list has a single entry, the spell draw menu will not be used, and casting will be immediate instead. |
| ChargesColor | Color | The color of the remaining charges bar. |
| ChargesBackColor | Color | The background color of the remaining charges bar. |
| AuraColor | Color | The color of the aura around the player when charging. |
| AddToWizardBook | bool | Whether the tool should be added to the Wizard's store. This requires the [Tool Upgraders](https://www.nexusmods.com/stardewvalley/mods/48379) mod installed to do anything. |