namespace Stripe
{
    using System;
    using System.Text;

    using Stripe.Infrastructure;

    /// <summary>
    /// This class contains utility methods to process event objects in Stripe's webhooks.
    /// </summary>
    public static class EventUtility
    {
        internal static readonly UTF8Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private const int DefaultTimeTolerance = 300;

        /// <summary>
        /// Parses a JSON string from a Stripe webhook into a <see cref="Event"/> object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="throwOnApiVersionMismatch">
        /// If <c>true</c> (default), the method will throw a <see cref="StripeException"/> if the
        /// API version of the event doesn't match Stripe.net's default API version (see
        /// <see cref="StripeConfiguration.ApiVersion"/>).
        /// </param>
        /// <returns>The deserialized <see cref="Event"/>.</returns>
        /// <exception cref="StripeException">
        /// Thrown if the API version of the event doesn't match Stripe.net's default API version.
        /// </exception>
        /// <remarks>
        /// This method doesn't verify <a href="https://stripe.com/docs/webhooks/signatures">webhook
        /// signatures</a>. It's recommended that you use
        /// <see cref="ConstructEvent(string, string, string, long, bool)"/> instead.
        /// </remarks>
        public static Event ParseEvent(string json, bool throwOnApiVersionMismatch = true)
        {
            var stripeEvent = JsonUtils.DeserializeObject<Event>(
                json,
                StripeConfiguration.SerializerSettings);

            if (throwOnApiVersionMismatch &&
                stripeEvent.ApiVersion != StripeConfiguration.ApiVersion)
            {
                throw new StripeException(
                    $"Received event with API version {stripeEvent.ApiVersion}, but Stripe.net "
                    + $"{StripeConfiguration.StripeNetVersion} expects API version "
                    + $"{StripeConfiguration.ApiVersion}. We recommend that you create a "
                    + "WebhookEndpoint with this API version. Otherwise, you can disable this "
                    + "exception by passing `throwOnApiVersionMismatch: false` to "
                    + "`Stripe.EventUtility.ParseEvent` or `Stripe.EventUtility.ConstructEvent`, "
                    + "but be wary that objects may be incorrectly deserialized.");
            }

            return stripeEvent;
        }

        /// <summary>
        /// Parses a JSON string from a Stripe webhook into a <see cref="Event"/> object, while
        /// verifying the <a href="https://stripe.com/docs/webhooks/signatures">webhook's
        /// signature</a>.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="stripeSignatureHeader">
        /// The value of the <c>Stripe-Signature</c> header from the webhook request.
        /// </param>
        /// <param name="validator">The webhook endpoint's signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <param name="utcNow">The timestamp to use for the current time.</param>
        /// <param name="throwOnApiVersionMismatch">
        /// If <c>true</c> (default), the method will throw a <see cref="StripeException"/> if the
        /// API version of the event doesn't match Stripe.net's default API version (see
        /// <see cref="StripeConfiguration.ApiVersion"/>).
        /// </param>
        /// <returns>The deserialized <see cref="Event"/>.</returns>
        /// <exception cref="StripeException">
        /// Thrown if the signature verification fails for any reason, of if the API version of the
        /// event doesn't match Stripe.net's default API version.
        /// </exception>
        public static Event ConstructEvent(
            string json,
            string stripeSignatureHeader,
            ISignatureValidator validator,
            long tolerance,
            long utcNow,
            bool throwOnApiVersionMismatch = true)
        {
            validator.Validate(json, stripeSignatureHeader, tolerance, utcNow);
            return ParseEvent(json, throwOnApiVersionMismatch);
        }

        /// <summary>
        /// Parses a JSON string from a Stripe webhook into a <see cref="Event"/> object, while
        /// verifying the <a href="https://stripe.com/docs/webhooks/signatures">webhook's
        /// signature</a>.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="stripeSignatureHeader">
        /// The value of the <c>Stripe-Signature</c> header from the webhook request.
        /// </param>
        /// <param name="secret">The webhook endpoint's signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds (default 300).</param>
        /// <param name="throwOnApiVersionMismatch">
        /// If <c>true</c> (default), the method will throw a <see cref="StripeException"/> if the
        /// API version of the event doesn't match Stripe.net's default API version (see
        /// <see cref="StripeConfiguration.ApiVersion"/>).
        /// </param>
        /// <returns>The deserialized <see cref="Event"/>.</returns>
        /// <exception cref="StripeException">
        /// Thrown if the signature verification fails for any reason, of if the API version of the
        /// event doesn't match Stripe.net's default API version.
        /// </exception>
        public static Event ConstructEvent(
            string json,
            string stripeSignatureHeader,
            string secret,
            long tolerance = DefaultTimeTolerance,
            bool throwOnApiVersionMismatch = true)
        {
            return ConstructEvent(
                json,
                stripeSignatureHeader,
                new DefaultSignatureValidator(secret),
                tolerance,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                throwOnApiVersionMismatch);
        }

        /// <summary>
        /// Parses a JSON string from a Stripe webhook into a <see cref="Event"/> object, while
        /// verifying the <a href="https://stripe.com/docs/webhooks/signatures">webhook's
        /// signature</a>.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="stripeSignatureHeader">
        /// The value of the <c>Stripe-Signature</c> header from the webhook request.
        /// </param>
        /// <param name="secret">The webhook endpoint's signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <param name="utcNow">The timestamp to use for the current time.</param>
        /// <param name="throwOnApiVersionMismatch">
        /// If <c>true</c> (default), the method will throw a <see cref="StripeException"/> if the
        /// API version of the event doesn't match Stripe.net's default API version (see
        /// <see cref="StripeConfiguration.ApiVersion"/>).
        /// </param>
        /// <returns>The deserialized <see cref="Event"/>.</returns>
        /// <exception cref="StripeException">
        /// Thrown if the signature verification fails for any reason, of if the API version of the
        /// event doesn't match Stripe.net's default API version.
        /// </exception>
        public static Event ConstructEvent(
            string json,
            string stripeSignatureHeader,
            string secret,
            long tolerance,
            long utcNow,
            bool throwOnApiVersionMismatch = true)
        {
            return ConstructEvent(
                json,
                stripeSignatureHeader,
                new DefaultSignatureValidator(secret),
                tolerance,
                utcNow,
                throwOnApiVersionMismatch);
        }

        public static void ValidateSignature(string json, string stripeSignatureHeader, string secret, long tolerance = DefaultTimeTolerance)
        {
            ValidateSignature(json, stripeSignatureHeader, secret, tolerance, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public static void ValidateSignature(string json, string stripeSignatureHeader, string secret, long tolerance, long utcNow)
        {
            var validator = new DefaultSignatureValidator(secret);
            validator.Validate(json, stripeSignatureHeader, tolerance, utcNow);
        }
    }
}
