using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;

namespace DelvCD.Helpers
{
    public class ClipRectsHelper
    {
        private List<ClipRect> _clipRects = new List<ClipRect>();

        private static List<byte[]> _ignoredAddonNames = new List<byte[]>()
        {
            "_FocusTargetInfo"u8.ToArray(),
        };

        public static bool IsIgnored(Span<byte> s)
        {
            foreach (byte[] ignored in _ignoredAddonNames)
            {
                if (s.SequenceEqual(ignored))
                {
                    return true;
                }
            }
            return false;
        }

        public unsafe void Update()
        {
            _clipRects.Clear();

            AtkStage* stage = AtkStage.Instance();
            if (stage == null) { return; }

            RaptureAtkUnitManager* manager = stage->RaptureAtkUnitManager;
            if (manager == null) { return; }

            AtkUnitList* loadedUnitsList = &manager->AtkUnitManager.AllLoadedUnitsList;

            Vector2 screenSize = ImGui.GetMainViewport().Size;

            for (var i = 0; i < loadedUnitsList->Count; i++)
            {
                try
                {
                    AtkUnitBase* addon = *(AtkUnitBase**)Unsafe.AsPointer(ref loadedUnitsList->Entries[i]);
                    if (addon == null || !addon->IsVisible || addon->WindowNode == null || addon->Scale == 0)
                    {
                        continue;
                    }

                    Span<byte> name = addon->Name;
                    if (IsIgnored(name))
                    {
                        continue;
                    }

                    var margin = 5 * addon->Scale;
                    var bottomMargin = 13 * addon->Scale;

                    Vector2 pos = new Vector2(addon->X + margin, addon->Y + margin);
                    Vector2 size = new Vector2(
                        addon->WindowNode->Width * addon->Scale - margin,
                        addon->WindowNode->Height * addon->Scale - bottomMargin
                    );
                    
                    // special case for duty finder
                    if (name.SequenceEqual("ContentsFinder"u8))
                    {
                        size.X += size.X + (16 * addon->Scale);
                        size.Y += (30 * addon->Scale);
                    }
                    
                    if (name.SequenceEqual("Journal"u8))
                    {
                        size.X += size.X + (16 * addon->Scale);
                    }

                    // just in case this causes weird issues / crashes (doubt it though...)
                    ClipRect clipRect = new ClipRect(pos, pos + size, screenSize);
                    if (clipRect.Max.X < clipRect.Min.X || clipRect.Max.Y < clipRect.Min.Y)
                    {
                        continue;
                    }

                    _clipRects.Add(clipRect);
                }
                catch { }
            }
        }

        public ClipRect? GetClipRectForArea(Vector2 pos, Vector2 size)
        {
            var area = new ClipRect(pos, pos + size, ImGui.GetMainViewport().Size);
            foreach (ClipRect clipRect in _clipRects)
            {
                if (clipRect.IntersectsWith(area))
                {
                    return clipRect;
                }
            }

            return null;
        }

        public bool IsPointClipped(Vector2 point)
        {
            foreach (ClipRect clipRect in _clipRects)
            {
                if (clipRect.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public readonly struct ClipRect
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public ClipRect(Vector2 min, Vector2 max, Vector2 screenSize)
        {
            Min = Clamp(min, Vector2.Zero, screenSize);
            Max = Clamp(max, Vector2.Zero, screenSize);
        }

        public bool Contains(Vector2 p)
        {
            return p.X <= Max.X && p.X >= Min.X && p.Y <= Max.Y && p.Y >= Min.Y;
        }

        public bool IntersectsWith(ClipRect other)
        {
            return other.Max.X >= Min.X && other.Min.X <= Max.X &&
                other.Max.Y >= Min.Y && other.Min.Y <= Max.Y;
        }

        private static Vector2 Clamp(Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(Math.Max(min.X, Math.Min(max.X, vector.X)), Math.Max(min.Y, Math.Min(max.Y, vector.Y)));
        }
    }
}
