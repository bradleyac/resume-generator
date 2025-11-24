# Features to be implemented

* ~~Posting lifecycle statuses (applied, archived)~~
* ~~UI for setting statuses~~
* ~~Port resume page to Blazor~~
* ~~Cover letter generation~~
* ~~Allow re-generating just the cover letter with additional feedback for the model.~~
* ~~Search across postings~~
* Goal setting
* More than one user
  Everything must be owned by a user
  Forbid actions on another user's resources
* Master resume data management interface
  Allow CRUD operations on:
    bullets
    books
    projects
    skills
* Make ingestion script work on sites other than Indeed
  ~~LinkedIn~~
  Dice
  Jobot
* Automate job posting submission/ingestion somehow (crawling with hosted headless browser, browser extension when on page, jobs API)
  long-term: automate searching and ingesting
  ~~short-term: Manually trigger automatic scraping of current page via JS~~
    ~~Should be able to include submission in the script to be run.~~