// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Marketplace.Commands.Product;
using AzureMcp.Marketplace.Models;

namespace AzureMcp.Marketplace.Commands;

[JsonSerializable(typeof(ProductGetCommand.ProductGetCommandResult))]
[JsonSerializable(typeof(ProductDetails))]
[JsonSerializable(typeof(ProductSummary))]
[JsonSerializable(typeof(PlanDetails))]
[JsonSerializable(typeof(PlanSummary))]
[JsonSerializable(typeof(StopSellInfo))]
[JsonSerializable(typeof(MarketingMaterial))]
[JsonSerializable(typeof(LinkProperties))]
[JsonSerializable(typeof(ImageGroup))]
[JsonSerializable(typeof(Artifact))]
[JsonSerializable(typeof(ProductVideo))]
[JsonSerializable(typeof(Availability))]
[JsonSerializable(typeof(BillingComponent))]
[JsonSerializable(typeof(PurchaseDurationDiscount))]
[JsonSerializable(typeof(TermUpn))]
[JsonSerializable(typeof(MarketStartPrice))]
[JsonSerializable(typeof(PlanMetadata))]
[JsonSerializable(typeof(Meter))]
[JsonSerializable(typeof(Term))]
[JsonSerializable(typeof(InvoicingPolicy))]
[JsonSerializable(typeof(Image))]
[JsonSerializable(typeof(PreviewImage))]
[JsonSerializable(typeof(PlanSkuRelation))]
[JsonSerializable(typeof(Price))]
[JsonSerializable(typeof(IncludedQuantityProperty))]
[JsonSerializable(typeof(TermDescriptionParameter))]
[JsonSerializable(typeof(ProrationPolicy))]
[JsonSerializable(typeof(BillingPlan))]
[JsonSerializable(typeof(FilterInstruction))]
[JsonSerializable(typeof(RelatedSku))]
[JsonSerializable(typeof(LegalTermsType))]
[JsonSerializable(typeof(AzureBenefit))]
[JsonSerializable(typeof(Badge))]
[JsonSerializable(typeof(PublisherType))]
[JsonSerializable(typeof(PublishingStage))]
[JsonSerializable(typeof(ProductType))]
[JsonSerializable(typeof(PricingType))]
[JsonSerializable(typeof(RatingBucket))]
[JsonSerializable(typeof(StopSellReason))]
[JsonSerializable(typeof(CspState))]
[JsonSerializable(typeof(VmArchitectureType))]
[JsonSerializable(typeof(VmSecurityType))]
[JsonSerializable(typeof(PricingAudience))]
[JsonSerializable(typeof(ArtifactType))]
[JsonSerializable(typeof(IList<string>))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
[JsonSerializable(typeof(IList<PlanDetails>))]
[JsonSerializable(typeof(IReadOnlyList<PlanSummary>))]
[JsonSerializable(typeof(IReadOnlyList<RatingBucket>))]
[JsonSerializable(typeof(IReadOnlyList<Badge>))]
[JsonSerializable(typeof(IReadOnlyList<PricingType>))]
[JsonSerializable(typeof(IReadOnlyList<VmSecurityType>))]
[JsonSerializable(typeof(IList<LinkProperties>))]
[JsonSerializable(typeof(IList<ImageGroup>))]
[JsonSerializable(typeof(IList<Artifact>))]
[JsonSerializable(typeof(IList<ProductVideo>))]
[JsonSerializable(typeof(IList<Availability>))]
[JsonSerializable(typeof(IList<BillingComponent>))]
[JsonSerializable(typeof(IList<PurchaseDurationDiscount>))]
[JsonSerializable(typeof(IList<Term>))]
[JsonSerializable(typeof(IList<FilterInstruction>))]
[JsonSerializable(typeof(IList<IncludedQuantityProperty>))]
[JsonSerializable(typeof(IList<Image>))]
[JsonSerializable(typeof(List<TermDescriptionParameter>))]
[JsonSerializable(typeof(ICollection<string>))]
[JsonSerializable(typeof(IDictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, int[]>))]
[JsonSerializable(typeof(IReadOnlyList<PlanSkuRelation>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
internal sealed partial class MarketplaceJsonContext : JsonSerializerContext;
