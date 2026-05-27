ALTER TABLE Tests ADD COLUMN QuoteId INTEGER REFERENCES Quotes(Id);
ALTER TABLE Tests ADD COLUMN CustomText TEXT;

CREATE INDEX IX_Tests_QuoteId ON Tests (QuoteId);
CREATE INDEX IX_Tests_CreatedAt ON Tests (CreatedAt DESC);

UPDATE Tests SET CustomText = TargetText;

UPDATE Tests SET TargetText = NULL WHERE QuoteId IS NOT NULL OR CustomText IS NOT NULL;
