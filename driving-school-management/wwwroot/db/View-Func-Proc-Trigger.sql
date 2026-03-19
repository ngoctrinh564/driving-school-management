-- ============================================================
-- ====================        AUTH        ====================
-- ============================================================
CREATE OR REPLACE PROCEDURE PROC_LOGIN
(
    p_username IN NVARCHAR2,
    o_userId OUT NUMBER,
    o_roleId OUT NUMBER,
    o_username OUT NVARCHAR2,
    o_password OUT NVARCHAR2,
    o_isActive OUT NUMBER
)
AS
BEGIN
    SELECT userId, roleId, userName, "password", isActive
    INTO o_userId, o_roleId, o_username, o_password, o_isActive
    FROM "User"
    WHERE userName = p_username;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        o_userId := NULL;
END;
/

CREATE OR REPLACE PROCEDURE PROC_REGISTER
(
    p_username IN NVARCHAR2,
    p_password IN NVARCHAR2,
    p_roleId IN NUMBER,
    o_result OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM "User"
    WHERE userName = p_username;

    IF v_count > 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    INSERT INTO "User"(roleId, userName, "password", isActive)
    VALUES (p_roleId, p_username, p_password, 1);

    o_result := 1;
END;
/

CREATE OR REPLACE PROCEDURE PROC_RESET_PASSWORD
(
    p_username IN NVARCHAR2,
    p_newPassword IN NVARCHAR2,
    o_result OUT NUMBER
)
AS
BEGIN
    UPDATE "User"
    SET "password" = p_newPassword
    WHERE userName = p_username;

    IF SQL%ROWCOUNT = 0 THEN
        o_result := -1;
    ELSE
        o_result := 1;
    END IF;
END;
/

-- ============================================================
-- ====================        ADMIN        ====================
-- ============================================================

CREATE OR REPLACE PROCEDURE PROC_ADMIN_DASHBOARD
(
    o_totalUser OUT NUMBER,
    o_totalHoSo OUT NUMBER,
    o_totalGPLX OUT NUMBER,
    o_totalBaiThi OUT NUMBER,
    o_userActive OUT NUMBER,
    o_userInactive OUT NUMBER,
    o_recentUsers OUT SYS_REFCURSOR
)
AS
BEGIN
    -- tổng user
    SELECT COUNT(*) INTO o_totalUser FROM "User";
    -- tổng hồ sơ
    SELECT COUNT(*) INTO o_totalHoSo FROM HoSoThiSinh;
    -- tổng GPLX
    SELECT COUNT(*) INTO o_totalGPLX FROM GiayPhepLaiXe;
    -- tổng bài thi
    SELECT COUNT(*) INTO o_totalBaiThi FROM BaiThi;
    -- user active
    SELECT COUNT(*) INTO o_userActive FROM "User" WHERE isActive = 1;
    -- user inactive
    SELECT COUNT(*) INTO o_userInactive FROM "User" WHERE isActive = 0;
    -- 5 user mới nhất
    OPEN o_recentUsers FOR
        SELECT *
        FROM (
            SELECT userId, userName, isActive
            FROM "User"
            ORDER BY userId DESC
        )
        WHERE ROWNUM <= 5;

END;
/





COMMIT;