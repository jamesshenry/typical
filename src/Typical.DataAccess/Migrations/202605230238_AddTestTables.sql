
CREATE TABLE Tests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CreatedAt INTEGER NOT NULL,
    Wpm REAL,
    RawWpm REAL,
    Accuracy REAL,
    DurationMs INTEGER,
    QuoteId INTEGER NULL REFERENCES Quotes(Id),
    CustomText TEXT NULL
);

CREATE INDEX IX_Tests_QuoteId ON Tests (QuoteId);
CREATE INDEX IX_Tests_CreatedAt ON Tests (CreatedAt DESC);

CREATE TABLE KeystrokeTelemetry (
    TestId INTEGER NOT NULL,
    OffsetMs INTEGER NOT NULL,
    GraphemeIndex INTEGER NOT NULL,
    ActualText TEXT,
    KeystrokeType INTEGER NOT NULL,
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
);

CREATE INDEX IX_KeystrokeTelemetry_TestId ON KeystrokeTelemetry(TestId);

CREATE TABLE TestSnapshots (
    TestId INTEGER NOT NULL,
    OffsetMs INTEGER NOT NULL,
    Wpm REAL NOT NULL,
    Accuracy REAL NOT NULL,
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
);
