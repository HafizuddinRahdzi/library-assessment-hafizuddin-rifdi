-- seed.sql
-- Insert 10 books
INSERT INTO Books (Title, Author, ISBN, PublishedYear, TotalCopies)
VALUES
('Alfu Sanatin', 'Ibu Rizal', 'ISBN 016-9-752-81239-2', 2025, 5),
('Alfu Sanatin 2', 'Ibu Rizal', 'ISBN 912-9-982-90725-3', 2026, 2),
('Design Patterns', 'Erich Gamma', 'ISBN 712-9-916-81235-1', 1994, 4),
('Refactoring', 'Martin Fowler', 'ISBN 463-9-267-87153-2', 1999, 2),
('Domain-Driven Design', 'Eric Evans', 'ISBN 723-9-678-98745-2', 2003, 6),
('Effective C#', 'Bill Wagner', 'ISBN 638-9-854-65279-2', 2010, 3),
('CLR via C#', 'Jeffrey Richter', 'ISBN 354-9-367-26846-2', 2012, 4),
('Head First Design Patterns', 'Eric Freeman', 'ISBN 204-9-984-98357-2', 2004, 5),
('Pro ASP.NET Core', 'Adam Freeman', 'ISBN 985-9-017-36715-2', 2020, 7),
('Entity Framework Core in Action', 'Jon Smith', 'ISBN 365-9-358-98357-2', 2018, 3);

-- Insert 5 members
INSERT INTO Members (SubjectId, FullName, Email, JoinedDate)
VALUES
('sub001', 'Alice Tan', 'alice@example.com', GETDATE()),
('sub002', 'Bob Lee', 'bob@example.com', GETDATE()),
('sub003', 'Charlie Wong', 'charlie@example.com', GETDATE()),
('sub004', 'Diana Lim', 'diana@example.com', GETDATE()),
('sub005', 'Ethan Ng', 'ethan@example.com', GETDATE());

-- Insert 10 loans (mix of returned and active)
INSERT INTO Loans (BookId, MemberId, BorrowedDate, ReturnedDate)
VALUES
(1, 1, DATEADD(DAY, -20, GETDATE()), NULL), -- active overdue
(2, 1, DATEADD(DAY, -5, GETDATE()), NULL), -- active
(3, 2, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -10, GETDATE())), -- returned
(4, 2, DATEADD(DAY, -15, GETDATE()), NULL), -- active
(5, 3, DATEADD(DAY, -40, GETDATE()), DATEADD(DAY, -5, GETDATE())), -- returned
(6, 3, DATEADD(DAY, -10, GETDATE()), NULL), -- active
(7, 4, DATEADD(DAY, -25, GETDATE()), DATEADD(DAY, -2, GETDATE())), -- returned
(8, 4, DATEADD(DAY, -3, GETDATE()), NULL), -- active
(9, 5, DATEADD(DAY, -50, GETDATE()), DATEADD(DAY, -20, GETDATE())), -- returned
(10, 5, DATEADD(DAY, -1, GETDATE()), NULL); -- active