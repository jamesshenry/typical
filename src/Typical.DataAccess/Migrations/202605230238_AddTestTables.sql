
CREATE TABLE Tests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CreatedAt INTEGER NOT NULL,
    Wpm REAL,
    RawWpm REAL,
    Accuracy REAL,
    DurationMs INTEGER,
    
    -- TargetText is now NULLABLE
    TargetText TEXT NULL, 
    
    -- New hybrid columns
    QuoteId INTEGER NULL REFERENCES Quotes(Id),
    CustomText TEXT NULL
);

CREATE TABLE KeystrokeTelemetry (
    TestId INTEGER NOT NULL,
    OffsetMs INTEGER NOT NULL,
    GraphemeIndex INTEGER NOT NULL,
    ActualText TEXT NOT NULL,
    KeystrokeType INTEGER NOT NULL,
    
    PRIMARY KEY (TestId, OffsetMs),
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
) WITHOUT ROWID;

CREATE TABLE TestSnapshots (
    TestId INTEGER NOT NULL,
    OffsetMs INTEGER NOT NULL,
    Wpm REAL NOT NULL,
    Accuracy REAL NOT NULL,
    PRIMARY KEY (TestId, OffsetMs),
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
) WITHOUT ROWID;

CREATE INDEX IX_Tests_QuoteId ON Tests (QuoteId);
CREATE INDEX IX_Tests_CreatedAt ON Tests (CreatedAt DESC);
