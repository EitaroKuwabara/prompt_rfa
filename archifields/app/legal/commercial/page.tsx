// archifields/app/legal/commercial/page.tsx

export default function CommercialPage() {
  return (
    <div className="container max-w-3xl py-10 px-6">
      <h1 className="text-3xl font-bold mb-6">
        Notation based on the Specified Commercial Transactions Act
      </h1>
      <div className="prose prose-slate dark:prose-invert">
        <h2 className="text-xl font-semibold mt-8 mb-2">Distributor</h2>
        <p>Mulberryfields</p>

        {/* <h2 className="text-xl font-semibold mt-8 mb-2">Representative</h2>
        <p>[Your Representative Name]</p> */}

        {/* <h2 className="text-xl font-semibold mt-8 mb-2">Address</h2>
        <p>[Zip Code], [Your Address], Gifu Prefecture, Japan</p> */}

        <h2 className="text-xl font-semibold mt-8 mb-2">Phone Number</h2>
        <p>[Your Phone Number]</p>
        <p className="text-sm text-muted-foreground">
          Reception hours: 10:00 - 18:00 (Excluding weekends and holidays)
        </p>

        {/* <h2 className="text-xl font-semibold mt-8 mb-2">Email Address</h2>
        <p>[Your Email Address]</p> */}

        <h2 className="text-xl font-semibold mt-8 mb-2">Website URL</h2>
        <p>https://archifields.com</p>

        <h2 className="text-xl font-semibold mt-8 mb-2">Selling Price</h2>
        <p>
          Indicated on each plan page (Currently offered for free during the
          Beta period).
        </p>

        <h2 className="text-xl font-semibold mt-8 mb-2">Additional Fees</h2>
        <p>
          Charges for internet connection and other telecommunications lines are
          borne by the customer.
        </p>

        <h2 className="text-xl font-semibold mt-8 mb-2">Payment Methods</h2>
        <p>Currently free of charge.</p>

        <h2 className="text-xl font-semibold mt-8 mb-2">
          Returns and Cancellations
        </h2>
        <p>
          Due to the nature of digital content, we do not accept returns or
          exchanges. If there are any defects in the Generated Data, please
          contact us via the support form.
        </p>
      </div>
    </div>
  );
}
