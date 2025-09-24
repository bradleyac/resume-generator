# Resume Generation System

## Overview

In a nutshell, the Resume Generation System generates 1-page, job-specific resumes and cover letters based on job description text and a master resume. It has an interface for browsing job postings that have been recorded to it and the artifacts associated with that job posting (resume, cover letter, anything else). It has a search mechanism to search the full text within the saved postings. Additionally, when a job posting is saved to the Resume Generation System a process is kicked off which generates a PDF resume and cover letter for that job posting, sending notifications when it is ready.

The MVP of the RGS will not integrate with any 3rd party systems, it will simply allow browsing job postings along with saved artifacts, generate and save the artifacts, and send notifications to the user.

## System Architecture

This will use a serverless architecture in the Azure cloud, making use of Azure App Service static sites and Azure Functions APIs. Artifacts will be stored in Azure Blob Storage. Job postings and artifact locations will be stored in a NoSQL document store using Azure CosmosDB serverless.

Job postings will be imported through the React frontend, which will store the job posting as a JSON document in the NoSQL store. The Azure CosmosDB change feed will trigger artifact generation for each job posting, and then trigger notifications when artifacts are created. The artifacts can be downloaded through the UI for use when applying to the job.

Paring down the master resume data to a 1-page resume tailored to the job, and any amount of cover letter generation, are AI tasks. Research still pending on how exactly to do this, but resume paring could be achieved by applying weights to the bullet points and then using the N best ones based on weight. Cover letter generation can probably be achieved with an Azure OpenAI service API call. Unsure whether a custom trained machine-learning model is required for good weights on the resume bullets, but GPT models suffice for cover letter generation.

## Misc

### Lifecycle of a Job Posting

When a job posting is imported, it is initially stored in the PendingPostings NoSQL container in Azure CosmosDB. The Azure CosmosDB change feed will notify Azure Functions that a document was added and trigger artifact generation for the job posting. When the artifacts (resume, cover letter) are complete, the posting is moved to the CompletedPostings NoSQL container and the artifacts can be downloaded from the frontend.

### Anatomy of a Resume

Resumes have information about me: 
  * Name
  * Email Address
  * Phone Number
  * Location
  * Headshot

Resumes have an objective statement

Resumes have information about my job history:
  * Name of company
  * Location of company
  * Years worked
  * Bullets

Resumes have information about my other projects:
  * Type of project 
  * Dates worked
  * Bullets

Resumes have information about my skills:
  * Backend
  * Frontend
  * DevOps
  * Cloud

Resumes have information about my education:
  * University
  * Location
  * Degree
  * Graduation Year