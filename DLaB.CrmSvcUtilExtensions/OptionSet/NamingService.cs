using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace DLaB.CrmSvcUtilExtensions.OptionSet
{
    using Transliteration;
    using Transliteration.Alphabets;

    public sealed class NamingService : INamingService
    {
        public NamingService(INamingService namingService)
        {
            DefaultNamingService = namingService;

            this._optionNames = new Dictionary<OptionSetMetadataBase,
                Dictionary<string, int>>();

            var russianDict = Russian.Alphabet;
            //1058
            //this.TransliterationAlphabet = new TransliterationAlphabet(1049, russianDict);
            this.TransliterationAlphabet = new TransliterationAlphabet(1058, russianDict);
        }

        #region INamingService

        /// <summary>
        /// Returns name of the OptionSet.
        /// </summary>
        /// <returns>Returns name of the OptionSet.</returns>
        public string GetNameForOptionSet(
            EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            // If this is global option set -- use it's system name.
            if (optionSetMetadata.IsGlobal.HasValue && optionSetMetadata.IsGlobal.Value)
            {
                return DefaultNamingService.GetNameForOptionSet(entityMetadata, optionSetMetadata, services);
            }

            // If it's option set on some entity -- concatenate entity name and option set name.
            // Ex: contact_facebookGenderCode

            // Find the attribute which uses the specified OptionSet.
            var attribute =
                entityMetadata.Attributes
                .FirstOrDefault(x =>
                    x.AttributeType == AttributeTypeCode.Picklist &&
                    ((EnumAttributeMetadata)x).OptionSet.MetadataId == optionSetMetadata.MetadataId);

            // Check for null, since statuscode attributes on custom entities are not
            // global, but their optionsets are not included in the attribute
            // metadata of the entity, either.
            if (attribute != null)
            {
                var nameForEntity = DefaultNamingService.GetNameForEntity(entityMetadata, services);

                var nameForAttribute = DefaultNamingService.GetNameForAttribute(entityMetadata, attribute, services);

                return string.Format("{0}_{1}", nameForEntity, nameForAttribute);
            }

            return DefaultNamingService.GetNameForOptionSet(entityMetadata, optionSetMetadata, services);
        }

        /// <summary>
        /// Returns name of the option in OptionSet.
        /// We implement our own version to handle a case when there is there is no English name.
        /// </summary>
        public string GetNameForOption(
            OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services)
        {
            var defaultName = DefaultNamingService.GetNameForOption(optionSetMetadata, optionMetadata, services);

            if (!string.IsNullOrEmpty(defaultName) && !defaultName.Contains("UnknownLabel"))
            {
                return defaultName;
            }

            var localName = optionMetadata.Label
                .LocalizedLabels
                .Where(x => x.LanguageCode == this.TransliterationAlphabet.LanguageCode)
                .Select(x => x.Label)
                .SingleOrDefault();

            var name = this.TransliterationAlphabet.Transliterate(localName);

            name = EnsureValidIdentifier(name);
            name = EnsureUniqueOptionName(optionSetMetadata, name);

            return name;
        }

        public string GetNameForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForEntity(entityMetadata, services);
        }

        public string GetNameForAttribute(
            EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);
        }

        public string GetNameForRelationship(
            EntityMetadata entityMetadata,
            RelationshipMetadataBase relationshipMetadata,
            EntityRole? reflexiveRole,
            IServiceProvider services)
        {
            return DefaultNamingService.GetNameForRelationship(
                entityMetadata, relationshipMetadata, reflexiveRole, services);
        }

        public string GetNameForServiceContext(IServiceProvider services)
        {
            return DefaultNamingService.GetNameForServiceContext(services);
        }

        public string GetNameForEntitySet(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForEntitySet(entityMetadata, services);
        }

        public string GetNameForMessagePair(SdkMessagePair messagePair, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForMessagePair(messagePair, services);
        }

        public string GetNameForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForRequestField(request, requestField, services);
        }

        public string GetNameForResponseField(
            SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForResponseField(response, responseField, services);
        }

        #endregion INamingService
        
        #region PRIVATE

        /// <summary>
        /// Returns number of time option with the same name has been defined.
        /// </summary>
        private readonly Dictionary<OptionSetMetadataBase, Dictionary<string, int>> _optionNames;

        private INamingService DefaultNamingService { get; set; }

        private TransliterationAlphabet TransliterationAlphabet { get; set; }

        /// <summary>
        /// Checks to make sure that the name begins with a valid character. If the name
        /// does not begin with a valid character, then add an underscore to the
        /// beginning of the name.
        /// </summary>
        private static string EnsureValidIdentifier(string name)
        {
            // Check to make sure that the option set begins with a word character
            // or underscore.
            var pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";
            if (!Regex.IsMatch(name, pattern))
            {
                // Prepend an underscore to the name if it is not valid.
                name = string.Format("_{0}", name);
                Trace.TraceInformation(string.Format("Name of the option changed to {0}",
                    name));
            }
            return name;
        }

        /// <summary>
        /// Checks to make sure that the name does not already exist for the OptionSet
        /// to be generated.
        /// </summary>
        private string EnsureUniqueOptionName(OptionSetMetadataBase metadata, string name)
        {
            if (this._optionNames.ContainsKey(metadata))
            {
                if (this._optionNames[metadata].ContainsKey(name))
                {
                    // Increment the number of times that an option with this name has
                    // been found.
                    ++this._optionNames[metadata][name];

                    // Append the number to the name to create a new, unique name.
                    var newName = string.Format("{0}_{1}",
                        name, this._optionNames[metadata][name]);

                    Trace.TraceInformation(string.Format(
                        "The {0} OptionSet already contained a definition for {1}. Changed to {2}",
                        metadata.Name, name, newName));

                    // Call this function again to make sure that our new name is unique.
                    return EnsureUniqueOptionName(metadata, newName);
                }
            }
            else
            {
                // This is the first time this OptionSet has been encountered. Add it to
                // the dictionary.
                this._optionNames[metadata] = new Dictionary<string, int>();
            }

            // This is the first time this name has been encountered. Begin keeping track
            // of the times we've run across it.
            this._optionNames[metadata][name] = 1;

            return name;
        }

        #endregion PRIVATE
    }
}
