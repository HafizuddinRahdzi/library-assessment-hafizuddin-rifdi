-- queries.sql

-- Q1
SELECT TOP 5 b.Title, b.Author, COUNT(l.Id) AS TimesBorrowed
FROM Books b
JOIN Loans l ON b.Id = l.BookId
GROUP BY b.Title, b.Author
ORDER BY TimesBorrowed DESC;

-- Q2
SELECT m.FullName, m.Email, COUNT(l.Id) AS OverdueLoans
FROM Members m
JOIN Loans l ON m.Id = l.MemberId
WHERE l.ReturnedDate IS NULL AND DATEDIFF(DAY, l.BorrowedDate, GETDATE()) > 14
GROUP BY m.FullName, m.Email;

-- Q3
WITH Months AS (
    SELECT DATEADD(MONTH, -n, CAST(GETDATE() AS DATE)) AS MonthStart
    FROM (SELECT TOP 12 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
          FROM sys.objects) AS Numbers
)
SELECT FORMAT(MonthStart, 'yyyy-MM') AS Month,
       COUNT(l.Id) AS TotalLoans
FROM Months
LEFT JOIN Loans l ON FORMAT(l.BorrowedDate, 'yyyy-MM') = FORMAT(MonthStart, 'yyyy-MM')
GROUP BY MonthStart
ORDER BY MonthStart;

-- Q4
SELECT b.Title, b.Author
FROM Books b
LEFT JOIN Loans l ON b.Id = l.BookId
WHERE l.Id IS NULL;

-- Q5
SELECT TOP 1 m.FullName, b.Title,
       DATEDIFF(DAY, l.BorrowedDate, l.ReturnedDate) AS DaysBorrowed
FROM Loans l
JOIN Members m ON l.MemberId = m.Id
JOIN Books b ON l.BookId = b.Id
WHERE l.ReturnedDate IS NOT NULL
ORDER BY DaysBorrowed DESC;