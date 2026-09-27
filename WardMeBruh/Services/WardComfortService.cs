using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;

namespace WardMeBruh.Services;

public static class WardComfortService
{
    private class CachedComfort
    {
        public int ComfortLevel;
        public float LastUpdateTime;
    }

    private static readonly System.Reflection.FieldInfo _allComfortPiecesField =
        AccessTools.Field(typeof(Piece), "s_allComfortPieces");

    private static readonly Dictionary<ZDOID, CachedComfort> _cache = new();
    private static readonly List<Piece> _tempPieces = new();

    public static int GetWardComfort(PrivateArea ward)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return 1;

        ZDOID wardId = ward.m_nview.GetZDO().m_uid;
        float now = Time.time;

        if (_cache.TryGetValue(wardId, out CachedComfort cached) && now - cached.LastUpdateTime < 3.0f)
        {
            return cached.ComfortLevel;
        }

        int calculated = CalculateWardMaxComfort(ward);
        _cache[wardId] = new CachedComfort
        {
            ComfortLevel = calculated,
            LastUpdateTime = now
        };

        return calculated;
    }

    public static void InvalidateCache(ZDOID wardId)
    {
        _cache.Remove(wardId);
    }

    private static int CalculateWardMaxComfort(PrivateArea ward)
    {
        float radius = WardQueryService.GetWardRadius(ward);
        List<Piece> validPieces = new List<Piece>();

        int ghostLayer = LayerMask.NameToLayer("ghost");
        HashSet<Piece> allPieces = null;
        if (_allComfortPiecesField != null)
        {
            try
            {
                allPieces = _allComfortPiecesField.GetValue(null) as HashSet<Piece>;
            }
            catch (System.Exception ex)
            {
                WardMeBruh.Log.Warning($"[Ward Comfort] Failed to access s_allComfortPieces: {ex.Message}");
            }
        }

        if (allPieces != null)
        {
            foreach (Piece piece in allPieces)
            {
                if (piece == null || piece.gameObject == null)
                    continue;

                if (ghostLayer != -1 && piece.gameObject.layer == ghostLayer)
                    continue;

                if (piece.GetComfort() <= 0)
                    continue;

                if (WardQueryService.IsInsideWard(ward, piece.transform.position))
                {
                    validPieces.Add(piece);
                }
            }
        }
        else
        {
            Vector3 wardPos = ward.transform.position;
            _tempPieces.Clear();
            Piece.GetAllComfortPiecesInRadius(wardPos, radius * 2f, _tempPieces);
            for (int i = 0; i < _tempPieces.Count; i++)
            {
                Piece piece = _tempPieces[i];
                if (piece == null || piece.gameObject == null)
                    continue;

                if (ghostLayer != -1 && piece.gameObject.layer == ghostLayer)
                    continue;

                if (piece.GetComfort() <= 0)
                    continue;

                if (WardQueryService.IsInsideWard(ward, piece.transform.position))
                {
                    validPieces.Add(piece);
                }
            }
        }

        validPieces.Sort(PieceComfortSort);

        int totalComfort = 2;

        for (int i = 0; i < validPieces.Count; i++)
        {
            Piece piece = validPieces[i];
            if (i > 0)
            {
                Piece prev = validPieces[i - 1];
                if ((piece.m_comfortGroup != Piece.ComfortGroup.None && piece.m_comfortGroup == prev.m_comfortGroup) || piece.m_name == prev.m_name)
                {
                    continue;
                }
            }

            totalComfort += piece.GetComfort();
        }

        WardMeBruh.Log.Debug($"[Ward Comfort] Ward {ward.m_nview.GetZDO().m_uid} calculated Max Comfort: {totalComfort} across {validPieces.Count} pieces (Radius: {radius}m)");
        return totalComfort;
    }

    private static int PieceComfortSort(Piece x, Piece y)
    {
        if (x.m_comfortGroup != y.m_comfortGroup)
        {
            return x.m_comfortGroup.CompareTo(y.m_comfortGroup);
        }
        float cX = x.GetComfort();
        float cY = y.GetComfort();
        if (!Mathf.Approximately(cX, cY))
        {
            return cY.CompareTo(cX);
        }
        return string.CompareOrdinal(y.m_name, x.m_name);
    }
}
