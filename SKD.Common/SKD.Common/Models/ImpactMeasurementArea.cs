using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Converters;
using Xamarin.Forms;
using Res = SKD.Common.Resources.AppResources;

namespace SKD.Common.Models
{
    public enum ImpactMeasurementArea
    {
        ChildHealth,
        CommunityRelations,
        ResponsibleLimits,
        Education,
        FamilyRelationships,
        PositiveValues,
        SocialIdentity,
        ConstructiveUseOfTime,
        SelfEsteemAndDreams,
        ChristianFaith
    }

    public static class ImpactMeasurementAreaExtensions
    {
        public static Color GetAccentColour(this ImpactMeasurementArea area)
        {
            string key = area.ToString() + "Accent";
            return (Color)Application.Current.Resources[Application.Current.Resources.MergedDictionaries.First().ContainsKey(key) ? key : "UserActionAccent"];
        }

        public static string GetGlyph(this ImpactMeasurementArea area)
        {
            string key = area.ToString() + "Icon";
            return (string)Application.Current.Resources[Application.Current.Resources.MergedDictionaries.First().ContainsKey(key) ? key : "UnknownIMAIcon"];
        }

        public static string GetLocalisedName(this ImpactMeasurementArea area) => area switch
        {
            ImpactMeasurementArea.PositiveValues => Res.PositiveValues,
            ImpactMeasurementArea.ConstructiveUseOfTime => Res.ConstructiveUseOfTime,
            ImpactMeasurementArea.Education => Res.Education,
            ImpactMeasurementArea.FamilyRelationships => Res.FamilyRelationships,
            ImpactMeasurementArea.CommunityRelations => Res.CommunityRelations,
            ImpactMeasurementArea.ChildHealth => Res.ChildHealth,
            ImpactMeasurementArea.SelfEsteemAndDreams => Res.SelfEsteemAndDreams,
            ImpactMeasurementArea.SocialIdentity => Res.SocialIdentity,
            ImpactMeasurementArea.ResponsibleLimits => Res.ResponsibleLimits,
            ImpactMeasurementArea.ChristianFaith => Res.ChristianFaith,
            _ => Res.UnknownIMA
        };

        public static int GetColourOrder(this ImpactMeasurementArea area) => (int)area;

    }

}
