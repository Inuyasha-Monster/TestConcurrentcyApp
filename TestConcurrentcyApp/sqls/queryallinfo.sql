--3. 查询所有同学的学号、姓名、选课数、总成绩；
select st.ID,st.Name,Count(sc.CourseID),sum(sc.Score) from [dbo].[Student] as st
left join [dbo].[Score] as sc on st.ID = sc.StudentID
group by st.ID,st.Name