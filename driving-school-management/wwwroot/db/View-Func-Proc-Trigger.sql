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

-- kiểm tra đã có những thông tin cần thiết trước khi đăng nhập vào
CREATE OR REPLACE FUNCTION FUNC_CHECK_USER_PROFILE
(
    p_userId IN NUMBER
)
RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM HOCVIEN
    WHERE USERID = p_userId
      AND EMAIL IS NOT NULL
      AND SDT IS NOT NULL
      AND SOCMNDCCCD IS NOT NULL;

    IF v_count = 0 THEN
        RETURN 0; -- chưa đủ
    END IF;

    RETURN 1; -- đầy đủ
END;
/
-- ============================================================
-- ====================        ADMIN        ====================
-- ============================================================
-- DASHBOARD
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
-- Exam: kì thi
CREATE OR REPLACE VIEW VW_KYTHI_ADMIN AS
SELECT 
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi,
    COUNT(ct.hoSoId) AS soLuongDangKy
FROM KyThi kt
LEFT JOIN ChiTietDangKyThi ct 
    ON kt.kyThiId = ct.kyThiId
GROUP BY 
    kt.kyThiId, 
    kt.tenKyThi, 
    kt.loaiKyThi;
    
/
CREATE OR REPLACE PROCEDURE SP_CREATE_KYTHI
(
    p_tenKyThi NVARCHAR2,
    p_loaiKyThi NVARCHAR2
)
AS
BEGIN
    INSERT INTO KyThi (tenKyThi, loaiKyThi)
    VALUES (p_tenKyThi, p_loaiKyThi);
END;
/
CREATE OR REPLACE PROCEDURE SP_UPDATE_KYTHI
(
    p_kyThiId NUMBER,
    p_tenKyThi NVARCHAR2,
    p_loaiKyThi NVARCHAR2
)
AS
BEGIN
    UPDATE KyThi
    SET 
        tenKyThi = p_tenKyThi,
        loaiKyThi = p_loaiKyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_DELETE_KYTHI
(
    p_kyThiId NUMBER
)
AS
BEGIN
    DELETE FROM KyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_KYTHI
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT * FROM VW_KYTHI_ADMIN
    ORDER BY kyThiId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_CREATE_LICHTHI
(
    p_kyThiId NUMBER,
    p_thoiGianThi TIMESTAMP,
    p_diaDiem NVARCHAR2
)
AS
BEGIN
    INSERT INTO LichThi (kyThiId, thoiGianThi, diaDiem)
    VALUES (p_kyThiId, p_thoiGianThi, p_diaDiem);
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_LICHTHI_BY_KYTHI
(
    p_kyThiId NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT *
    FROM LichThi
    WHERE kyThiId = p_kyThiId
    ORDER BY thoiGianThi;
END;
/






COMMIT;