--取出 31-40 的数据 其中ID不是连续的
--利用 id not in 的方式取出来，过滤不要的
select top 10 * from [dbo].[Teacher] as t
where t.ID not in(
select top 30 t1.ID from  [dbo].[Teacher] as t1 order by t1.ID
)
order by t.ID

--利用 id > max(id) 的方式
select top 10 * from [dbo].[Teacher] as t
where t.ID>(
	select MAX(t1.ID) from (
		select top 30 t2.ID as ID from [dbo].[Teacher] as t2 order by t2.ID
	) as t1
)
order by t.ID

-- 利用 RowNumber() over(order by ID)
SELECT TOP 10 * FROM(
  SELECT ROW_NUMBER() OVER (ORDER BY t.ID) AS RowNumber,* FROM [dbo].[Teacher]  as t
) A
WHERE A.RowNumber > 30