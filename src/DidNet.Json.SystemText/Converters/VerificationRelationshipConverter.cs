using DidNet.Common.Verification;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DidNet.Json.SystemText
{
    /// <summary>
    /// Converts a <see cref="VerificationRelationship"/> to or from JSON.
    /// </summary>
    /// <typeparam name="TVerificationRelationship">The type of <see cref="VerificationRelationship"/> to convert.</typeparam>
    public class VerificationRelationshipConverter<TVerificationRelationship>: JsonConverter<TVerificationRelationship> where TVerificationRelationship : VerificationRelationship
    {
        /// <inheritdoc/>
        public override TVerificationRelationship Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.StartObject)
            {
                ThrowHelper.ThrowJsonException();
            }

            //TODO: Need to add "JwsVerificationKey2020" and handle like verification relationship?
            //This is an extension: https://identity.foundation/sidetree/spec/v0.1.0/
            // https://w3c-ccg.github.io/ld-cryptosuite-registry/
            object? constructorParameter = null;
            if(reader.TokenType == JsonTokenType.String)
            {
                constructorParameter = reader.GetString() ?? string.Empty;
            }
            else if(reader.TokenType == JsonTokenType.StartObject)
            {
                constructorParameter = JsonSerializer.Deserialize<VerificationMethod>(ref reader, options);
            }
            else
            {
                //Make the reason be an unexpected token JSON token type.
                ThrowHelper.ThrowJsonException();
            }

            return (TVerificationRelationship)Activator.CreateInstance(typeof(TVerificationRelationship), new object[] { constructorParameter! })!;
        }


        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TVerificationRelationship value, JsonSerializerOptions options)
        {
            var converter = GetKeyConverter<VerificationMethod>(options);
            var verification = (VerificationRelationship)value;

            if(verification.IsEmbeddedVerification && verification.EmbeddedVerification != null)
            {
                //TODO: Fix this.
#pragma warning disable CS8604 // Possible null reference argument.
                converter.Write(writer, value.EmbeddedVerification, options);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else if(!verification.IsEmbeddedVerification)
            {
                writer.WriteStringValue(verification.VerificationReferenceId);
            }
        }


        private static JsonConverter<TVerificationMethod> GetKeyConverter<TVerificationMethod>(JsonSerializerOptions options) where TVerificationMethod : VerificationMethod
        {
            return (JsonConverter<TVerificationMethod>)options.GetConverter(typeof(TVerificationMethod));
        }
    }
}
