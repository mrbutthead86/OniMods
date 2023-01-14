using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DiscoverChain
{
  [HarmonyPatch(typeof(DiscoveredResources))]
  [HarmonyPatch("Discover")]
  public static class DiscoverPatch
  {
    private const string defaultConfigFileName = "config.default.json";
    private const string configFileName = "config.json";

    private static readonly Internals.Logger logger = new Internals.Logger(nameof(DiscoverChain));
    private static IDictionary<string, IEnumerable<string>> chainConfig;

    public static void Postfix(Tag tag, Tag categoryTag)
    {
      LoadConfig();
      DiscoverByChain(tag);
    }

    private static void LoadConfig()
    {
      if (chainConfig != null)
        return;

      string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      string configFilePath = Path.Combine(dir, configFileName);
      if (!File.Exists(configFilePath))
        File.Copy(Path.Combine(dir, defaultConfigFileName), configFilePath);

      string chainConfigAsString = File.ReadAllText(configFilePath);
      chainConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<string>>>(chainConfigAsString);

      logger.Log($"Config loaded with {chainConfig.Count} entries");
    }

    private static void DiscoverByChain(Tag tag)
    {
      if (chainConfig.TryGetValue(tag.ToString(), out IEnumerable<string> tagsToDiscover))
      {
        logger.Log($"Starting discovery chain ({tagsToDiscover.Join()}) for {tag}.");
        tagsToDiscover.ToList().ForEach(tagToDiscover => Discover((Tag)tagToDiscover));
      }
    }

    private static void Discover(Tag tag)
    {
      if (!DiscoveredResources.Instance.IsDiscovered(tag))
      {
        if (TryDetermineCategoryTag(tag, out Tag? categoryTag))
          DiscoveredResources.Instance.Discover(tag, categoryTag.Value);
      }
    }

    private static bool TryDetermineCategoryTag(Tag tag, out Tag? categoryTag)
    {
      GameObject prefab = Assets.TryGetPrefab(tag);
      categoryTag = default;
      if (prefab)
      {
        categoryTag = DiscoveredResources.GetCategoryForEntity(prefab.GetComponent<KPrefabID>());
        logger.Log($"Category tag {categoryTag} was determined for {tag}");
      }
      else
        logger.Warn($"No prefab found for {tag}");

      return categoryTag != null;
    }
  }
}
