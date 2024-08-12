# SQLite Fulltext Search Example #

This repository implements a Fulltext Search Engine using SQLite FTS5 Tables. You can use it to index documents, 
such as PDF files. It comes with a Blazor Frontend built upon the FluentUI Component library. 

## What's included ##

There is a page for searching documents and displaying the matches. The search results highlight the matching text:

<a href="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_SearchResults.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_SearchResults.jpg" alt="The final Document Search with the Blazor Frontend" width="100%" />
</a>

We are using suggestions to give it a Google-like experience and make it easier to find documents by Keywords:

<a href="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_Suggestions.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_Suggestions.jpg" alt="Search Suggestions" width="100%" />
</a>

There is also a page to upload documents and assign Keywords, using a Token Input:

<a href="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_Upload.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/SqliteFulltextSearch/master/doc/img/SqliteFulltextSearch_Upload.jpg" alt="Document Upload" width="100%" />
</a>

## Getting Started ##

Getting started is as simple as cloning this repository and running the following command:

```
docker compose --profile dev up
```

You can then navigate to `https://localhost:5001` and start searching and indexing documents.

## License ##

All code is released under terms of the [MIT License].

[MIT License]: https://opensource.org/licenses/MIT
