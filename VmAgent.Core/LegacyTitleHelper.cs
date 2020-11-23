// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;
    using System.Collections.Generic;

    public static class LegacyTitleHelper
    {
        public static readonly IReadOnlyDictionary<Guid, LegacyTitleDetails> LegacyTitleMappings = new Dictionary<Guid, LegacyTitleDetails>
        {
            {
                // Titanfall Xbox one Party server
                new Guid("ffffffff-ffff-ffff-b9b1-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 52622311,
                    GsiSetId = new Guid("e2195adb-263a-484b-b992-fbcbdaca52f7"),
                    VariantId = new Guid("3b091194-2543-498a-97bd-3d2ad1210bce"),
                    GsiId = new Guid("786ddaef-0d9e-4b77-929c-5820324750ad")
                }
            },
            {
                // Titanfall Xbox one Training server
                new Guid("ffffffff-ffff-ffff-47ee-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 1703142257,
                    GsiSetId = new Guid("112e58e7-8c75-443d-b046-9e5bd000128a"),
                    VariantId = new Guid("397720bb-ba84-48f3-a433-7bd5c8246707"),
                    GsiId = new Guid("cf55df78-f14f-42d9-b785-52cf7a7d5322")
                }
            },
            {
                // Titanfall Xbox one Game server
                new Guid("ffffffff-ffff-ffff-4f7f-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 1997886388,
                    GsiSetId = new Guid("d47adc39-ee15-49ce-82b6-6627d7d374f3"),
                    VariantId = new Guid("4c80172e-1ffc-43fe-b9cf-3f9f8592c66d"),
                    GsiId = new Guid("4c979483-8fe8-4fab-968e-2ab5a9c869c3")
                }
            },
            {
                // Titanfall Xbox 360 Party server
                new Guid("ffffffff-ffff-ffff-34cc-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 359726680,
                    GsiSetId = new Guid("1e54ae1e-e360-4c90-9ad1-b181c5f9e14c"),
                    VariantId = new Guid("d4e0ef33-1795-4893-bf7a-969cd3a537f5"),
                    GsiId = new Guid("ca83d8a7-1378-4685-ab9c-315d60d48bbc")
                }
            },
            {
                // Titanfall Xbox 360 Training server
                new Guid("ffffffff-ffff-ffff-e113-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 1675069585,
                    GsiSetId = new Guid("2cb541e0-c8ef-4e72-847b-00639c002316"),
                    VariantId = new Guid("4c9d0449-2af0-4cbd-991c-a745bc4f4a3d"),
                    GsiId = new Guid("d4699ce3-02ac-4b64-8500-af02bd890b04")
                }
            },
            {
                // Titanfall Xbox 360 Game server
                new Guid("ffffffff-ffff-ffff-9334-000000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 1128644651,
                    GsiSetId = new Guid("8d0f50ae-4f33-4e12-8ab9-ce85c75b69ce"),
                    VariantId = new Guid("e2870662-986c-4bf5-8e43-5ea8776cf726"),
                    GsiId = new Guid("aa5d22ed-bdd4-44b1-9053-7394fb652118")
                }
            },
            {
                // Call of Duty Black Ops 3
                new Guid("ffffffff-ffff-ffff-de7c-0b0000000000"),
                new LegacyTitleDetails
                {
                    TitleId = 604296285,
                    GsiSetId = new Guid("1f915313-5617-4536-8ec8-c5dab587441c"),
                    VariantId = new Guid("7b3f6803-be2d-48f2-85dd-17fb416dc3f3"),
                    GsiId = new Guid("7e823389-e065-4fec-8813-1fea9992962e")
                }
            }
        };
    }

    public class LegacyTitleDetails
    {
        public long TitleId { get; set; }

        public Guid GsiSetId { get; set; }

        public Guid VariantId { get; set; }

        public Guid GsiId { get; set; }
    }
}
