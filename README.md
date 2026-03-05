# Blackbird.io TAUS EPIC

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

TAUS EPIC helps users assess the quality of machine-translated content and make informed decisions about its suitability for various purposes.

## Before setting up

Before you can connect you need to make sure that:

- You have a TAUS account.
- You have an API key corresponding to your TAUS account. You can find API keys [here](https://www.taus.net/user/epic-api/keys)

## Connecting

1. Navigate to apps and search for TAUS EPIC.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My TAUS connection'.
4. Fill in the Base URL for your TAUS connection. This is most likely either `https://api.taus.net` or `https://api.sandbox.taus.net`
5. Fill in the API key for your TAUS account.
6. Click _Connect_.

![TAUSBlackbirdConnection](image/README/1714471684106.png)

## Actions

### Review

- **Review** Review translated content and output quality scores for each segment. Advanced settings:
  - **Target language**: Override the target language used for quality estimation.
  - **Output file handling**: Choose the output file format for downstream steps.
- **Review text** Review translated text and output a quality score.

### Editing

- **Edit** Edit translated content and output updated segments with quality metadata. Advanced settings:
  - **APE threshold**: Set the upper threshold used for APE processing.
  - **APE low threshold**: Set the lower threshold used for APE processing.
  - **Use RAG**: Enable retrieval-augmented generation during editing.
- **Edit text** Edit translated text and output an improved target text.
- **Edit in background** Edit translated content in background jobs and output job IDs for later retrieval. Advanced settings:
  - **Source language**: Override the source language used for job processing.
  - **Segment states to estimate**: Include only units that have segments in the selected states.
  - **Exclude segment state qualifiers**: Exclude segments that contain the specified state qualifiers.
- **Download background files** Download files processed in background jobs and output updated files with usage details. Advanced settings:
  - **State to set above threshold**: Set the segment state applied when the score is above the threshold.

## Events

### Batch polling

- **On background job finished** Triggered when all provided jobs reach a terminal status (completed, failed, or expired).

> Supported languages can be found [here](https://developer.taus.net/). TAUS' sandbox environment only supports :
> - English (en)
> - French (fr)
> - German (de)
> - Italian (it)
> - Spanish (es)

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
