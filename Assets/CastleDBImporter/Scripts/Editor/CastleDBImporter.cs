﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using SimpleJSON;
using System;
using System.Collections.Generic;

namespace CastleDBImporter
{
	[ScriptedImporter(1, "cdb")]
	public class CastleDBImporter : ScriptedImporter
	{
        private CastleDBParser parser = null;
        private string dbname = "";

		public override void OnImportAsset(AssetImportContext ctx)
		{
            if (HasDuplicateDB(ref ctx)) { Debug.LogWarning("Cannot load CastleDB database '" + ctx.assetPath + "' because a DB of the same name already exists! Please rename one of your .cdb files!"); return; }
            dbname = Path.GetFileNameWithoutExtension(ctx.assetPath);

            TextAsset castle = new TextAsset(File.ReadAllText(ctx.assetPath));
			ctx.AddObjectToAsset("main obj", castle);
			ctx.SetMainObject(castle);

			parser = new CastleDBParser(castle);
            EditorApplication.delayCall += new EditorApplication.CallbackFunction(GenerateTypes); // Delay type generation until the asset manager has finished importing
        }

        private void GenerateTypes()
        {
            CastleDBGenerator.GenerateTypes(parser.Root, CastleDBConfig.Instance() , dbname );
            parser = null;
            dbname = "";
        }

        private bool HasDuplicateDB(ref AssetImportContext ctx)
        {
            var dbname = Path.GetFileNameWithoutExtension(ctx.assetPath);

            var dupes = AssetDatabase.FindAssets(dbname);

            // Check all guids for dupe cdb files
            foreach (string guid1 in dupes)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid1);
                if(path.ToLower().EndsWith(".cdb") && path != ctx.assetPath)
                {
                    return true;
                }
            }

            return false;
        }
    }

}