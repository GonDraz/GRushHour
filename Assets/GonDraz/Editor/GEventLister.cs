using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GonDraz.Events;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace GonDraz.Editor
{
    public class GEventLister : EditorWindow
    {
        private readonly List<EventInfoEntry> _cachedEvents = new();
        private readonly Dictionary<string, bool> _paramFilters = new();
        private string _paramFilterSearch = "";

        private Vector2 _scroll;
        private string _searchQuery = "";
        private bool _showFilterBox;

        private void OnEnable()
        {
            RefreshCache();

            // Auto refresh khi project thay đổi hoặc code compile lại
            EditorApplication.projectChanged += RefreshCache;
            AssemblyReloadEvents.afterAssemblyReload += RefreshCache;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshCache;
            AssemblyReloadEvents.afterAssemblyReload -= RefreshCache;
        }

        private void OnGUI()
        {
            Space(5);

            BeginHorizontal();
            Label("Search:", Width(50));
            _searchQuery = TextField(_searchQuery);
            EndHorizontal();

            Space(5);
            _showFilterBox = EditorGUILayout.Foldout(_showFilterBox, "Filter by Parameter Type(s)");
            if (_showFilterBox)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                BeginHorizontal();
                Label("Param Type Filter Search:", Width(170));
                _paramFilterSearch = TextField(_paramFilterSearch);
                EndHorizontal();

                Space(4);

                var needle = _paramFilterSearch.ToLower();
                foreach (var key in _paramFilters.Keys.OrderBy(k => k).ToList()
                             .Where(key => string.IsNullOrEmpty(needle) || key.ToLower().Contains(needle)))
                    _paramFilters[key] = EditorGUILayout.ToggleLeft(key, _paramFilters[key]);

                Space(6);
                BeginHorizontal();
                if (Button("Select All")) SetAllFilters(true);
                if (Button("Clear All")) SetAllFilters(false);
                EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            Space(10);

            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            Label("Field Name", EditorStyles.boldLabel, Width(200));
            Label("Parameter Type(s)", EditorStyles.boldLabel, Width(200));
            Label("File Path", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var ev in _cachedEvents)
            {
                // search filter
                if (!string.IsNullOrEmpty(_searchQuery))
                {
                    var lower = _searchQuery.ToLower();
                    if (!ev.FieldName.ToLower().Contains(lower) &&
                        !ev.ParamTypes.Any(p => p.ToLower().Contains(lower)) &&
                        !ev.FilePath.ToLower().Contains(lower))
                        continue;
                }

                // checkbox filter (AND logic)
                if (_paramFilters.Any(f => f.Value))
                {
                    var selected = _paramFilters.Where(f => f.Value).Select(f => f.Key).ToList();
                    if (!selected.All(s => ev.ParamTypes.Contains(s)))
                        continue;
                }

                EditorGUILayout.BeginHorizontal();

                if (Button(ev.FieldName, EditorStyles.linkLabel, Width(200)))
                    OpenAt(ev);

                Label(string.Join(", ", ev.ParamTypes), Width(200));

                if (Button(Path.GetFileName(ev.FilePath), EditorStyles.linkLabel))
                    OpenAt(ev);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        [MenuItem("GonDraz/GEvent Lister")]
        public static void ShowWindow()
        {
            GetWindow<GEventLister>("GEvent Lister");
        }

        private void RefreshCache()
        {
            // preserve old filter states
            var prev = new Dictionary<string, bool>(_paramFilters);

            _cachedEvents.Clear();
            _paramFilters.Clear();

            // find EventManager files
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            var managerPaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => File.Exists(p) && File.ReadAllText(p).Contains("class EventManager"))
                .ToList();

            // pre-scan all EventManager files into dictionary<fieldName, (path,line)>
            var fieldLookup = new Dictionary<string, (string, int)>();
            foreach (var path in managerPaths)
            {
                var lines = File.ReadAllLines(path);
                foreach (var field in typeof(BaseEventManager).GetFields(BindingFlags.Static | BindingFlags.Public |
                                                                     BindingFlags.NonPublic))
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (!lines[i].Contains(field.Name)) continue;
                        fieldLookup[field.Name] = (path, i + 1);
                        break;
                    }
            }

            // reflection fields
            var fields =
                typeof(BaseEventManager).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!IsGEventType(field.FieldType)) continue;

                var paramTypes = GetParamTypes(field.FieldType);

                (string path, int line) info = ("(not found)", 1);
                if (fieldLookup.TryGetValue(field.Name, out var fi)) info = fi;

                _cachedEvents.Add(new EventInfoEntry
                {
                    FieldName = field.Name,
                    ParamTypes = paramTypes,
                    FilePath = info.path,
                    LineNumber = info.line
                });

                // add từng param vào filter
                foreach (var pt in paramTypes)
                {
                    var selected = prev.GetValueOrDefault(pt, false);
                    _paramFilters[pt] = selected;
                }
            }

            Repaint();
        }

        private static bool IsGEventType(Type t)
        {
            if (t == typeof(GEvent)) return true;
            return t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("GEvent");
        }

        private static List<string> GetParamTypes(Type t)
        {
            if (!t.IsGenericType) return new List<string> { "void" };
            return t.GetGenericArguments().Select(FriendlyTypeName).ToList();
        }

        private static string FriendlyTypeName(Type t)
        {
            if (!t.IsGenericType) return t.Name;
            var name = t.Name;
            var tick = name.IndexOf('`');
            if (tick >= 0) name = name[..tick];
            return $"{name}<{string.Join(", ", t.GetGenericArguments().Select(FriendlyTypeName))}>";
        }

        private void SetAllFilters(bool state)
        {
            foreach (var key in _paramFilters.Keys.ToList())
                _paramFilters[key] = state;
        }

        private static void OpenAt(EventInfoEntry ev)
        {
            if (ev.FilePath == "(not found)") return;
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(ev.FilePath);
            if (!script) return;
            EditorGUIUtility.PingObject(script);
            AssetDatabase.OpenAsset(script, ev.LineNumber);
        }

        private class EventInfoEntry
        {
            public string FieldName;
            public string FilePath;
            public int LineNumber;
            public List<string> ParamTypes;
        }
    }
}