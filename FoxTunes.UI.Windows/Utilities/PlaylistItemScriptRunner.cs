﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Utilities
{
    public class LibraryItemScriptRunner
    {
        public LibraryItemScriptRunner(IScriptingContext scriptingContext, LibraryItem libraryItem, string script)
        {
            this.ScriptingContext = scriptingContext;
            this.LibraryItem = libraryItem;
            this.Script = script;
        }

        public IScriptingContext ScriptingContext { get; private set; }

        public LibraryItem LibraryItem { get; private set; }

        public string Script { get; private set; }

        public void Prepare()
        {
            var metaData = new Dictionary<string, object>();
            foreach (var item in this.LibraryItem.MetaDatas)
            {
                var key = item.Name.ToLower();
                if (metaData.ContainsKey(key))
                {
                    //Not sure what to do. Doesn't happen often.
                    continue;
                }
                metaData.Add(key, item.Value);
            }

            var properties = new Dictionary<string, object>();
            foreach (var item in this.LibraryItem.Properties)
            {
                properties.Add(item.Name.ToLower(), item.Value);
            }
            var statistics = new Dictionary<string, object>();
            foreach (var item in this.LibraryItem.Statistics)
            {
                statistics.Add(item.Name.ToLower(), item.Value);
            }
            this.ScriptingContext.SetValue("playing", StandardManagers.Instance.Playback.CurrentStream);
            this.ScriptingContext.SetValue("item", this.LibraryItem);
            this.ScriptingContext.SetValue("tag", metaData);
            this.ScriptingContext.SetValue("prop", properties);
            this.ScriptingContext.SetValue("stat", statistics);
        }

        public object Run()
        {
            const string RESULT = "__result";
            try
            {
                this.ScriptingContext.Run(string.Concat("var ", RESULT, " = ", this.Script, ";"));
            }
            catch (Exception e)
            {
                return e;
            }
            return this.ScriptingContext.GetValue(RESULT);
        }
    }
}
