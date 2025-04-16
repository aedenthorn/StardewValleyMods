using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using Object = StardewValley.Object;

namespace WeatherTotem
{
	public partial class ModEntry
	{
		public class Object_rainTotem_Patch
		{
			public static bool Prefix(Object __instance, Farmer who)
			{
				if (!Config.ModEnabled)
					return true;

				GameLocation currentLocation = who.currentLocation;
				string locationContextId = currentLocation.GetLocationContextId();
				LocationContextData locationContext = currentLocation.GetLocationContext();

				if (!locationContext.AllowRainTotem)
				{
					return true;
				}
				if (locationContext.RainTotemAffectsContext is not null)
				{
					locationContextId = locationContext.RainTotemAffectsContext;
					locationContext = LocationContexts.Require(locationContext.RainTotemAffectsContext);
				}
				who.currentLocation.ShowPagedResponses(SHelper.Translation.Get("menu.prompt"), GetWeathersResponses(locationContext), (string response) => OnResponse(response, __instance, who, currentLocation, locationContextId));
				return false;
			}

			private static List<KeyValuePair<string, string>> GetWeathersResponses(LocationContextData locationContext)
			{
				List<KeyValuePair<string, string>> responses = new();
				Dictionary<string, int> weatherPriority = new()
				{
					{ Game1.weather_sunny, 0 },
					{ Game1.weather_rain, 1 },
					{ Game1.weather_green_rain, 2 },
					{ Game1.weather_lightning, 3 },
					{ Game1.weather_snow, 4 },
					{ Game1.weather_debris, 5 }
				};

				foreach (WeatherCondition weatherCondition in locationContext.WeatherConditions)
				{
					if (weatherCondition.Weather.Equals(Game1.weather_festival) || weatherCondition.Weather.Equals(Game1.weather_wedding))
						continue;

					if (!responses.Any(r => r.Key.Equals(weatherCondition.Weather)))
					{
						bool isConditionFulfilled = weatherCondition.Condition is null;

						if (!isConditionFulfilled)
						{
							string[] array = weatherCondition.Condition.Trim().Split(',', StringSplitOptions.TrimEntries);

							for (int i = 0; i < array.Length; i++)
							{
								if (array[i].Equals("IS_GREEN_RAIN_DAY"))
								{
									array[i] = "SEASON summer";
								}
							}
							isConditionFulfilled = !array.Any(condition => condition.StartsWith("SEASON") || condition.StartsWith("!SEASON")) || array.Any(condition =>
							{
								if (condition.StartsWith("SEASON") || condition.StartsWith("!SEASON"))
								{
									WorldDate worldDate = new(Game1.Date);
									worldDate.TotalDays++;
									string[] seasons = ArgUtility.SplitBySpace(condition);
									bool reverse = condition.StartsWith("!SEASON");
									bool anyMatch = seasons.Any(s => s.ToLower() == worldDate.SeasonKey);

									return (anyMatch && !reverse) || (reverse && !anyMatch);
								}
								return false;
							});
						}

						if (isConditionFulfilled)
						{
							responses.Add(new(weatherCondition.Weather, weatherCondition.Weather switch
							{
								Game1.weather_sunny => SHelper.Translation.Get("menu.Sun"),
								Game1.weather_rain => SHelper.Translation.Get("menu.Rain"),
								Game1.weather_green_rain => SHelper.Translation.Get("menu.GreenRain"),
								Game1.weather_lightning => SHelper.Translation.Get("menu.Storm"),
								Game1.weather_snow => SHelper.Translation.Get("menu.Snow"),
								Game1.weather_debris => SHelper.Translation.Get("menu.Wind"),
								_ => weatherCondition.Weather
							}));
						}
					}
				}
				responses.Sort((x, y) =>
				{
					return (weatherPriority.ContainsKey(x.Key), weatherPriority.ContainsKey(y.Key)) switch
					{
						(true, true) => weatherPriority[x.Key].CompareTo(weatherPriority[y.Key]),
						(true, false) => -1,
						(false, true) => 1,
						_ => x.Key.CompareTo(y.Key)
					};
				});
				return responses;
			}

			private static void OnResponse(string response, Object __instance, Farmer who, GameLocation currentLocation, string locationContextId)
			{
				string sound = response switch
				{
					Game1.weather_rain => Config.RainSound,
					Game1.weather_green_rain => Config.GreenRainSound,
					Game1.weather_lightning => Config.StormSound,
					Game1.weather_snow => Config.SnowSound,
					Game1.weather_debris => Config.WindSound,
					_ => Config.SunSound
				};
				bool flag = false;

				if (locationContextId.Equals("Default"))
				{
					if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
					{
						Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = response;
						flag = true;
					}
				}
				else
				{
					currentLocation.GetWeather().WeatherForTomorrow = response;
					flag = true;
				}
				if (flag)
				{
					Game1.pauseThenMessage(2000, SHelper.Translation.GetTranslations().Any(t => t.Key.Equals($"message.{response}")) ? SHelper.Translation.Get($"message.{response}") : SHelper.Translation.Get($"message.Default"));
				}
				Game1.screenGlow = false;
				if (!string.IsNullOrEmpty(Config.InvokeSound))
				{
					currentLocation.playSound(Config.InvokeSound);
				}
				who.canMove = false;
				Game1.screenGlowOnce(Color.SlateBlue, hold: false);
				Game1.player.faceDirection(2);
				Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
				{
					new(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
				});
				for (int i = 0; i < 6; i++)
				{
					Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
					{
						motion = new Vector2(Game1.random.Next(-10, 11) / 10f, -2f),
						delayBeforeAnimationStart = i * 200
					});
					Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
					{
						motion = new Vector2(Game1.random.Next(-30, -10) / 10f, -1f),
						delayBeforeAnimationStart = 100 + i * 200
					});
					Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
					{
						motion = new Vector2(Game1.random.Next(10, 30) / 10f, -1f),
						delayBeforeAnimationStart = 200 + i * 200
					});
				}
				TemporaryAnimatedSprite temporaryAnimatedSprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
				{
					motion = new Vector2(0f, -7f),
					acceleration = new Vector2(0f, 0.1f),
					scaleChange = 0.015f,
					alpha = 1f,
					alphaFade = 0.0075f,
					shakeIntensity = 1f,
					initialPosition = Game1.player.Position + new Vector2(0f, -96f),
					xPeriodic = true,
					xPeriodicLoopTime = 1000f,
					xPeriodicRange = 4f,
					layerDepth = 1f
				};
				temporaryAnimatedSprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
				Game1.Multiplayer.broadcastSprites(currentLocation, temporaryAnimatedSprite);
				DelayedAction.playSoundAfterDelay(sound, 2000);
				who.reduceActiveItemByOne();
			}
		}

		public class Object_performUseAction_Patch
		{
			public static void Postfix(Object __instance, ref bool __result)
			{
				if (__instance.Name is not null && __instance.name.Contains("Totem") && __instance.QualifiedItemId.Equals("(O)681"))
				{
					__result = false;
				}
			}
		}
	}
}
