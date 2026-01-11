import React from "react";

export default function PrivacyPage() {
  return (
    <div className="container max-w-3xl py-10 px-6">
      <h1 className="text-3xl font-bold mb-6">Privacy Policy</h1>
      <div className="prose prose-slate dark:prose-invert">
        <p className="mb-4 text-sm text-muted-foreground">
          Effective Date: 2026-01-11
        </p>

        <p>
          Mulberryfields (hereinafter referred to as the
          &quot;Company&quot;) establishes this Privacy Policy (hereinafter
          referred to as the &quot;Policy&quot;) regarding the handling of
          users&apos; personal information in the service
          &quot;Archifields&quot; (hereinafter referred to as the
          &quot;Service&quot;).
        </p>

        <h2 className="text-xl font-semibold mt-8 mb-4">
          1. Information We Collect
        </h2>
        <p>The Company may collect the following information:</p>
        <ul className="list-disc pl-6 space-y-2">
          <li>
            Personal identification information (Name, email address, etc.)
            provided during registration.
          </li>
          <li>
            Input data (prompts) and generation history used within the Service.
          </li>
          <li>Usage data and cookies to improve user experience.</li>
        </ul>

        <h2 className="text-xl font-semibold mt-8 mb-4">2. Purpose of Use</h2>
        <p>
          The Company uses the collected information for the following purposes:
        </p>
        <ul className="list-disc pl-6 space-y-2">
          <li>To provide and operate the Service.</li>
          <li>To improve the quality of the AI models and the Service.</li>
          <li>To respond to user inquiries.</li>
          <li>To prevent fraudulent or unauthorized use.</li>
        </ul>

        <h2 className="text-xl font-semibold mt-8 mb-4">
          3. Provision to Third Parties
        </h2>
        <p>
          The Company will not provide personal information to third parties
          without the user&apos;s consent, except as permitted by the Act on the
          Protection of Personal Information or other laws and regulations.
        </p>

        <h2 className="text-xl font-semibold mt-8 mb-4">4. Contact Us</h2>
        <p>
          For inquiries regarding this Policy, please contact from contact page.
        </p>
      </div>
    </div>
  );
}
