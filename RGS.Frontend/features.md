# Features to be implemented

* ~~Allow re-generating just the cover letter with additional feedback for the model.~~
  Keep a history of feedback given for a particular posting.
    Event sourcing opportunity?
* Goal setting?
    Move this idea to ADHD helper/productivity app?
* More than one user
  ~~Everything must be owned by a user~~
  ~~Forbid actions on another user's resources~~
  Each user gets an API key and can view it
* Master resume data management interface
  ~~Allow CRUD operations on:~~
    ~~bullets~~
    ~~books~~
    ~~projects~~
    ~~skills~~
    contact
  Polish more
    ~~Field widths need another look~~
    ~~Resume bullets are a little different than skills or technologies because they are sentences. Are badges right for them?~~
    ~~Padding~~
    ~~Titles and alignment~~
    Better experience creating new items with more than one field (jobs, projects, books)
* Make ingestion script work on sites other than Indeed
  ~~LinkedIn~~
  Dice
  Jobot
* Automate job posting submission/ingestion somehow (crawling with hosted headless browser, browser extension when on page, jobs API)
  long-term: automate searching and ingesting
  ~~short-term: Manually trigger automatic scraping of current page via JS~~
    ~~Should be able to include submission in the script to be run.~~
* Automated tests:
    Integration tests for every endpoint
    Integration tests for change feed processing
    Unit tests
    Start doing TDD?
* Generate practice interview questions based on job description
* Better loading experiences with placeholders or spinners or something.
* Free-form interactions with chatgpt around a particular job posting?



# TODOs
s
* Figure out how to do IaC for CosmosDB settings changes/container creation. I should not be remembering what I did in DEV and applying it to PROD.
* Identify places where transient errors could occur and mitigate them
    Not just HttpClient: CosmosClient and AzureOpenAIClient as well. CosmosClient may do some retries on gets already, but not create/update, so those may need to be handled.
* Standardize request validation
* More explicit authentication checks--don't rely on exceptions from usage of IUserService.GetCurrentUserId returning null.
    Would be easy enough to add an attribute for the anonymous-allowed endpoints
      AND/OR handle the x-api-key header in middleware? Then we can explicitly reject requests without an authenticated user or api key before it gets to controller.
        Do we need to differentiate between endpoints when allowing x-api-key instead of easy auth? Should all endpoints be accessible to a user via api key or only the expected ones?
          Principle of least privilege -> Only allow api-key auth for the import endpoint. Then a leaked api-key is not just as privileged as an authenticated user session.