﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace WeaverCore.Utilities
{
    /*public static class HarmonyInternal
    {
        static Assembly HarmonyAssembly;
        static Type HarmonyInstanceType;
        static Type HarmonyMethodType;

        static MethodInfo CreateInstanceMethod;
        static MethodInfo PatchMethod;
        static MethodInfo PatchAllMethod;

        static HarmonyInternal()
        {
            if (HarmonyAssembly == null)
            {
                HarmonyAssembly = Assembly.Load("0Harmony");
                HarmonyInstanceType = HarmonyAssembly.GetType("Harmony.HarmonyInstance");
                HarmonyMethodType = HarmonyAssembly.GetType("Harmony.HarmonyMethod");

                PatchMethod = HarmonyInstanceType.GetMethod("Patch", BindingFlags.Instance | BindingFlags.Public);

                CreateInstanceMethod = HarmonyInstanceType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                PatchAllMethod = HarmonyInstanceType.GetMethod("PatchAll", new Type[] { typeof(Assembly) });
            }
        }

        static HarmonyPatcher Create(string id)
        {
            return new HarmonyPatcher(CreateInstanceMethod.Invoke(null, new object[] { id }));
        }

        static DynamicMethod Patch(HarmonyPatcher instance, MethodBase original, MethodInfo prefix, MethodInfo postfix)
        {
            var prefixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { prefix });
            var postfixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { postfix });
            return (DynamicMethod)PatchMethod.Invoke(instance.harmonyInstance, new object[] { original, prefixInstance, postfixInstance, null });
        }
    }*/

    public struct HarmonyPatcher
    {
        static Assembly HarmonyAssembly;
        static Type HarmonyInstanceType;
        static Type HarmonyMethodType;

        static MethodInfo CreateInstanceMethod;
        static MethodInfo PatchMethod;
        static MethodInfo PatchAllMethod;

        static HarmonyPatcher()
        {
            if (HarmonyAssembly == null)
            {
#if UNITY_EDITOR
                HarmonyAssembly = Assembly.Load("0Harmony");
#else
                HarmonyAssembly = ResourceLoader.LoadAssembly("0Harmony");
#endif
                HarmonyInstanceType = HarmonyAssembly.GetType("Harmony.HarmonyInstance");
                HarmonyMethodType = HarmonyAssembly.GetType("Harmony.HarmonyMethod");

                PatchMethod = HarmonyInstanceType.GetMethod("Patch", BindingFlags.Instance | BindingFlags.Public);

                CreateInstanceMethod = HarmonyInstanceType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                PatchAllMethod = HarmonyInstanceType.GetMethod("PatchAll", new Type[] { typeof(Assembly) });
            }
        }

        object harmonyInstance;

        HarmonyPatcher(object HarmonyInstance)
        {
            harmonyInstance = HarmonyInstance;
        }

        public override bool Equals(object obj)
        {
            return obj is HarmonyPatcher && ((HarmonyPatcher)obj).harmonyInstance == harmonyInstance;
        }
        public override int GetHashCode()
        {
            return harmonyInstance.GetHashCode();
        }
        public override string ToString()
        {
            return harmonyInstance.ToString();
            //return id;
        }

        public static bool operator ==(HarmonyPatcher left, HarmonyPatcher right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HarmonyPatcher left, HarmonyPatcher right)
        {
            return !(left == right);
        }

        public static HarmonyPatcher Create(string id)
        {
            return new HarmonyPatcher(CreateInstanceMethod.Invoke(null, new object[] { id }));
        }

        public DynamicMethod Patch(MethodBase original, MethodInfo prefix, MethodInfo postfix)
        {
            if (harmonyInstance == null)
            {
                throw new Exception("This patcher is null");
            }
            if (prefix == null && postfix == null)
            {
                return null;
            }
            if (original == null)
            {
                throw new ArgumentNullException("original", "Method is null");
            }

            /*if (prefix == null)
            {
                throw new ArgumentNullException("prefix", "Method is null");
            }

            if (postfix == null)
            {
                throw new ArgumentNullException("postfix", "Method is null");
            }*/
            //return HarmonyInternal.Patch(this, original, prefix, postfix);
            var prefixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { prefix });
            var postfixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { postfix });
            return (DynamicMethod)PatchMethod.Invoke(harmonyInstance, new object[] { original, prefixInstance, postfixInstance, null });
        }

        /*public DynamicMethod Patch(Delegate original, Delegate prefix, Delegate postfix)
        {
            return Patch(original?.Method,prefix?.Method,postfix?.Method);
        }

        public DynamicMethod Patch(MethodBase original, Delegate prefix, Delegate postfix)
        {
            return Patch(original, prefix?.Method, postfix?.Method);
        }*/

        /*public DynamicMethod Patch(string original, string prefix, string postfix)
        {
            return Patch(MethodUtilities.GetMethod(original), MethodUtilities.GetMethod(original), MethodUtilities.GetMethod(original));
        }*/
    }
}
