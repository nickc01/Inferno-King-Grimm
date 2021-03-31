using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WeaverCore.Attributes;
using WeaverCore.Configuration;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public abstract class WeaverMod : Mod
    {
        //As soon as any WeaverCore mod loads, the init functions will be called
        //static WeaverMod()
        //{
           //WeaverLog.Log("RUNNING INIT");
           //Initializers.Initializers.OnGameInitialize();
        //}


        bool firstLoad = true;

        public IEnumerable<Registry> Registries
        {
            get
            {
                return Registry.FindModRegistries(GetType());
            }
        }

        /*static string Prettify(string input)
        {
            return Regex.Replace(input, @"(\S)([A-Z])", @"$1 $2");
        }*/

        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        public override void Initialize()
        {
            var modType = GetType();
            WeaverLog.Log("Initializing Mod = " + modType.FullName);
            base.Initialize();
            if (firstLoad)
            {
                //WeaverLog.Log("FIRST LOAD");
                firstLoad = false;
                Initializers.Initializers.OnGameInitialize();

                RegistryLoader.LoadAllRegistries(modType);

                ReflectionUtilities.ExecuteMethodsWithAttribute<AfterModLoadAttribute>((_, a) => a.ModType.IsAssignableFrom(modType));

                //WeaverLog.Log(modType.FullName + "___LOADED!!!");

                //Load all global mod settings pertaining to this mod
                /*foreach (var registry in Registry.FindModRegistries(modType))
                {
                    var settingTypes = registry.GetFeatureTypes<GlobalWeaverSettings>();
                    foreach (var settingsType in settingTypes)
                    {
                        settingsType.Load();
                    }
                }*/
            }
            else
            {
                EnableRegistries();
            }
        }

        public void EnableRegistries()
        {

            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = true;
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = false;
            }
        }
	}

    public abstract class TogglableWeaverMod : WeaverMod, ITogglableMod
    {
        public virtual void Unload()
        {
            DisableRegistries();
        }
    }
}
