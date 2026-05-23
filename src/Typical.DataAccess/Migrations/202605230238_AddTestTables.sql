
CREATE TABLE Tests (
    Id INTEGER PRIMARY KEY,
    CreatedAt INTEGER NOT NULL,
    Wpm REAL NOT NULL,
    RawWpm REAL NOT NULL,
    Accuracy REAL NOT NULL,
    DurationMs INTEGER NOT NULL,
    TargetText TEXT NOT NULL,
    Source TEXT
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
