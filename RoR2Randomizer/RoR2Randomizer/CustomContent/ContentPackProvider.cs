﻿using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.CustomContent
{
    public class ContentPackProvider : IContentPackProvider
    {
        internal readonly ContentPack contentPack = new ContentPack();

        public string identifier => Main.PluginGUID;

        public void Init()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;

            contentPack.entityStateTypes.Add(new Type[] { typeof(MultiEntityState) });

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}