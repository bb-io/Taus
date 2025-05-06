# Blackbird.io Taus

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

TAUS EPIC API helps users assess the quality of machine-translated content and make informed decisions about its suitability for various purposes.

## Before setting up

Before you can connect you need to make sure that:

- You have a TAUS account.
- You have an API key corresponding to your TAUS account. You can find API keys [here](https://www.taus.net/user/epic-api/keys)

## Connecting

1. Navigate to apps and search for TAUS.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My TAUS connection'.
4. Fill in the Base URL for your TAUS connection. This is most likely either `https://api.taus.net` or `https://api.sandbox.taus.net`
5. Fill in the API key for your TAUS account.
6. Click _Connect_.

![TAUSBlackbirdConnection](image/README/1714471684106.png)

## Actions

### Segments

- **Estimate** Gets quality estimation data for a single segment specifying: - Source text - Source language - Target text - Target language -
  Supported languages can be found [here](https://developer.taus.net/). TAUS' sandbox environment only supports :

- English (en)
- French (fr)
- German (de)
- Italian (it)
- Spanish (es)

### XLIFF files

- **Estimate XLIFF** Gets quality estimation data for all segments in an XLIFF 1.2 file. Mandatory input data include the translated XLIFF file, source and target language values. As an output, an Average Score is provided, which is the average of the quality scores for all segments in the file. Additionally, all translation units get appeneded with their individual score which is added to the `extradata` attribute in the XLIFF file. Optionally, you can add Threshold, New Target State and Condition input parameters to the Blackbird action to change the target state value of segments meeting the desired criteria.

Examples:

Setting the optional input values as shown in the image below will result in all segments with a score above 0.9 to have their target state values updated to "final".

![TAUSoptionalinput1](image/README/TAUSoptionalinput1.png)

Similarly, the setup below will cause all the segments with a quality score below or equal to 0.6 to have their target state value changed to "needs-review-translation".

![TAUSoptionalinput2](image/README/TAUSoptionalinput2.png)

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
