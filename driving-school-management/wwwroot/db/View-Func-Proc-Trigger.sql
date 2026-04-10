-- ============================================================
-- ====================        AUTH        ====================
-- ============================================================
CREATE OR REPLACE PROCEDURE PROC_LOGIN
(
    p_username IN NVARCHAR2,
    o_userId   OUT NUMBER,
    o_roleId   OUT NUMBER,
    o_username OUT NVARCHAR2,
    o_password OUT NVARCHAR2,
    o_isActive OUT NUMBER
)
AS
BEGIN
    BEGIN
        SELECT u.userId,
               u.roleId,
               u.userName,
               u."password",
               u.isActive
        INTO   o_userId,
               o_roleId,
               o_username,
               o_password,
               o_isActive
        FROM "User" u
        LEFT JOIN HocVien hv ON hv.userId = u.userId
        WHERE u.userName = p_username
           OR hv.email = p_username
        FETCH FIRST 1 ROWS ONLY;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            o_userId   := NULL;
            o_roleId   := NULL;
            o_username := NULL;
            o_password := NULL;
            o_isActive := NULL;
    END;
END;
/

CREATE OR REPLACE PROCEDURE PROC_REGISTER
(
    p_username IN NVARCHAR2,
    p_password IN NVARCHAR2,
    p_email    IN NVARCHAR2,
    p_roleId   IN NUMBER,
    o_result   OUT NUMBER
)
AS
    v_count_user  NUMBER;
    v_count_email NUMBER;
    v_userId      NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count_user
    FROM "User"
    WHERE userName = p_username;

    IF v_count_user > 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    SELECT COUNT(*)
    INTO v_count_email
    FROM HocVien
    WHERE email = p_email;

    IF v_count_email > 0 THEN
        o_result := -2;
        RETURN;
    END IF;

    INSERT INTO "User"(roleId, userName, "password", isActive)
    VALUES (p_roleId, p_username, p_password, 1)
    RETURNING userId INTO v_userId;

    INSERT INTO HocVien
    (
        hoTen,
        soCmndCccd,
        namSinh,
        gioiTinh,
        sdt,
        email,
        avatarUrl,
        userId
    )
    VALUES
    (
        p_username,
        'UNKNOWN',
        NULL,
        NULL,
        NULL,
        p_email,
        NULL,
        v_userId
    );

    o_result := 1;

EXCEPTION
    WHEN OTHERS THEN
        o_result := 0;
        RAISE;
END;
/

CREATE OR REPLACE PROCEDURE PROC_VERIFY_REGISTER_OTP
(
    p_email    IN NVARCHAR2,
    p_otpCode  IN NVARCHAR2,
    o_result   OUT NUMBER
)
AS
    v_pendingId PendingRegister.pendingId%TYPE;
    v_username  PendingRegister.userName%TYPE;
    v_password  PendingRegister."password"%TYPE;
    v_roleId    PendingRegister.roleId%TYPE;
    v_userId    NUMBER;
    v_count     NUMBER;
BEGIN
    BEGIN
        SELECT pendingId, userName, "password", roleId
        INTO v_pendingId, v_username, v_password, v_roleId
        FROM PendingRegister
        WHERE email = p_email
          AND otpCode = p_otpCode
          AND isUsed = 0
          AND expiredAt >= SYSDATE
        FETCH FIRST 1 ROWS ONLY;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            o_result := -1;
            RETURN;
    END;

    SELECT COUNT(*) INTO v_count
    FROM "User"
    WHERE userName = v_username;

    IF v_count > 0 THEN
        o_result := -2;
        RETURN;
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM HocVien
    WHERE email = p_email;

    IF v_count > 0 THEN
        o_result := -3;
        RETURN;
    END IF;

    INSERT INTO "User"(roleId, userName, "password", isActive)
    VALUES (v_roleId, v_username, v_password, 1)
    RETURNING userId INTO v_userId;

    INSERT INTO HocVien
    (
        hoTen,
        soCmndCccd,
        namSinh,
        gioiTinh,
        sdt,
        email,
        avatarUrl,
        userId
    )
    VALUES
    (
        v_username,
        'UNKNOWN',
        NULL,
        NULL,
        NULL,
        p_email,
        NULL,
        v_userId
    );

    UPDATE PendingRegister
    SET isUsed = 1
    WHERE pendingId = v_pendingId;

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
-- ====================        ADMIN        ===================
-- ============================================================
-- DASHBOARD
CREATE OR REPLACE PROCEDURE PROC_ADMIN_DASHBOARD
(
    o_totalUser         OUT NUMBER,
    o_totalHoSo         OUT NUMBER,
    o_totalGPLX         OUT NUMBER,
    o_totalBaiThi       OUT NUMBER,
    o_userActive        OUT NUMBER,
    o_userInactive      OUT NUMBER,
    o_hoSoDaDuyet       OUT NUMBER,
    o_hoSoDangXuLy      OUT NUMBER,
    o_recentUsers       OUT SYS_REFCURSOR,
    o_chartMonth        OUT SYS_REFCURSOR,
    o_chartHangGplx     OUT SYS_REFCURSOR
)
AS
BEGIN
    SELECT COUNT(*) INTO o_totalUser
    FROM "User";

    SELECT COUNT(*) INTO o_totalHoSo
    FROM HoSoThiSinh;

    SELECT COUNT(*) INTO o_totalGPLX
    FROM GiayPhepLaiXe;

    SELECT COUNT(*) INTO o_totalBaiThi
    FROM BaiThi;

    SELECT COUNT(*) INTO o_userActive
    FROM "User"
    WHERE isActive = 1;

    SELECT COUNT(*) INTO o_userInactive
    FROM "User"
    WHERE isActive = 0;

    SELECT COUNT(*) INTO o_hoSoDaDuyet
    FROM HoSoThiSinh
    WHERE UPPER(TRIM(trangThai)) = UPPER('Đã duyệt');

    SELECT COUNT(*) INTO o_hoSoDangXuLy
    FROM HoSoThiSinh
    WHERE UPPER(TRIM(trangThai)) = UPPER('Đang xử lý');

    OPEN o_recentUsers FOR
        SELECT
            userId,
            userName,
            isActive
        FROM "User"
        ORDER BY userId DESC
        FETCH FIRST 5 ROWS ONLY;

    OPEN o_chartMonth FOR
        SELECT
            TO_CHAR(m.month_num, 'FM00') AS THANG,
            NVL(hs.so_ho_so, 0) AS SO_HO_SO,
            NVL(dt.doanh_thu, 0) AS DOANH_THU
        FROM
        (
            SELECT LEVEL AS month_num
            FROM dual
            CONNECT BY LEVEL <= 12
        ) m
        LEFT JOIN
        (
            SELECT
                EXTRACT(MONTH FROM ngayDangKy) AS thang,
                COUNT(*) AS so_ho_so
            FROM HoSoThiSinh
            WHERE ngayDangKy IS NOT NULL
            GROUP BY EXTRACT(MONTH FROM ngayDangKy)
        ) hs
            ON m.month_num = hs.thang
        LEFT JOIN
        (
            SELECT
                EXTRACT(MONTH FROM ngayLap) AS thang,
                SUM(NVL(tongTien, 0)) AS doanh_thu
            FROM PhieuThanhToan
            WHERE ngayLap IS NOT NULL
            GROUP BY EXTRACT(MONTH FROM ngayLap)
        ) dt
            ON m.month_num = dt.thang
        ORDER BY m.month_num;

    OPEN o_chartHangGplx FOR
        SELECT
            hg.tenHang AS TENHANG,
            COUNT(hs.hoSoId) AS SO_LUONG
        FROM HangGplx hg
        LEFT JOIN HoSoThiSinh hs
            ON hg.hangId = hs.hangId
        GROUP BY hg.tenHang
        ORDER BY hg.tenHang;
END;
/
-- Exam: kì thi
CREATE OR REPLACE VIEW VW_KYTHI_ADMIN AS
SELECT
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi,
    REGEXP_SUBSTR(kt.tenKyThi, '\(([^)]+)\)$', 1, 1, NULL, 1) AS maHang,
    COUNT(DISTINCT ctdk.hoSoId) AS soLuongDangKy
FROM KyThi kt
LEFT JOIN ChiTietDangKyThi ctdk
    ON kt.kyThiId = ctdk.kyThiId
GROUP BY
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi;
/
CREATE OR REPLACE VIEW VW_KYTHI_HOCVIEN_DANGKY AS
SELECT DISTINCT
    kt.kyThiId,
    kt.tenKyThi,
    hs.hoSoId,
    hv.hocVienId,
    hv.hoTen,
    hv.sdt,
    hv.email,
    ctdk.lichThiId,
    ptt.phieuId,
    ptt.tenPhieu,
    ptt.ngayLap,
    ptt.ngayNop,
    ptt.phuongThuc
FROM KyThi kt
JOIN ChiTietDangKyThi ctdk
    ON kt.kyThiId = ctdk.kyThiId
JOIN HoSoThiSinh hs
    ON ctdk.hoSoId = hs.hoSoId
JOIN HocVien hv
    ON hs.hocVienId = hv.hocVienId
JOIN ChiTietPhieuThanhToan ctptt
    ON ctptt.hoSoId = hs.hoSoId
   AND ctptt.ketQuaHocTapId IS NULL
JOIN PhieuThanhToan ptt
    ON ptt.phieuId = ctptt.phieuId
WHERE ptt.ngayNop IS NOT NULL
  AND INSTR(ptt.tenPhieu, '_') > 0
  AND SUBSTR(ptt.tenPhieu, 1, INSTR(ptt.tenPhieu, '_') - 1) = kt.tenKyThi;
/
CREATE OR REPLACE PACKAGE PKG_KYTHI AS

    PROCEDURE GET_HANG_GPLX(
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE CREATE_KYTHI(
        p_hangId NUMBER,
        p_dot NUMBER,
        p_nam NUMBER
    );

    PROCEDURE GET_KYTHI(
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE DELETE_KYTHI(
        p_kyThiId NUMBER
    );

    PROCEDURE GET_LICHTHI_BY_KYTHI(
        p_kyThiId NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE AUTO_PHANCONG_LICHTHI(
        p_kyThiId NUMBER
    );

    PROCEDURE GET_HOCVIEN_DANGKY(
        p_kyThiId NUMBER,
        p_keyword NVARCHAR2,
        p_page NUMBER,
        p_pageSize NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE GET_HOCVIEN_DANGKY_COUNT(
        p_kyThiId NUMBER,
        p_keyword NVARCHAR2,
        p_total OUT NUMBER
    );
    
    PROCEDURE GET_KYTHI_EDIT_INFO(
        p_kyThiId NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );
    
    PROCEDURE UPDATE_CAP_KYTHI(
        p_kyThiId NUMBER,
        p_hangId NUMBER,
        p_dot NUMBER,
        p_nam NUMBER
    );

END PKG_KYTHI;
/
CREATE OR REPLACE PACKAGE BODY PKG_KYTHI AS

    TYPE T_NUMBER_TAB IS TABLE OF NUMBER INDEX BY PLS_INTEGER;

    PROCEDURE GET_HANG_GPLX(
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT
            hangId,
            maHang,
            tenHang
        FROM HangGplx
        ORDER BY hangId;
    END;

    PROCEDURE CREATE_KYTHI(
        p_hangId NUMBER,
        p_dot NUMBER,
        p_nam NUMBER
    )
    IS
        v_tenHang HangGplx.tenHang%TYPE;
        v_maHang HangGplx.maHang%TYPE;
        v_tenTotNghiep NVARCHAR2(255);
        v_tenSatHach NVARCHAR2(255);
        v_count NUMBER;
    BEGIN
        SELECT tenHang, maHang
        INTO v_tenHang, v_maHang
        FROM HangGplx
        WHERE hangId = p_hangId;

        v_tenTotNghiep := 'Kỳ thi hạng ' || v_tenHang || ' - Tốt nghiệp - đợt ' || p_dot || ' năm ' || p_nam || ' (' || v_maHang || ')';
        v_tenSatHach := 'Kỳ thi hạng ' || v_tenHang || ' - Sát hạch - đợt ' || p_dot || ' năm ' || p_nam || ' (' || v_maHang || ')';

        SELECT COUNT(*)
        INTO v_count
        FROM KyThi
        WHERE tenKyThi IN (v_tenTotNghiep, v_tenSatHach);

        IF v_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Ky thi da ton tai');
        END IF;

        INSERT INTO KyThi (tenKyThi, loaiKyThi)
        VALUES (v_tenTotNghiep, N'Tốt nghiệp');

        INSERT INTO KyThi (tenKyThi, loaiKyThi)
        VALUES (v_tenSatHach, N'Sát hạch');
    END;

    PROCEDURE GET_KYTHI(
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT
            kyThiId,
            tenKyThi,
            loaiKyThi,
            maHang,
            soLuongDangKy
        FROM VW_KYTHI_ADMIN
        ORDER BY kyThiId DESC;
    END;

    PROCEDURE DELETE_KYTHI(
        p_kyThiId NUMBER
    )
    IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*)
        INTO v_count
        FROM ChiTietDangKyThi
        WHERE kyThiId = p_kyThiId;

        IF v_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Ky thi da co hoc vien dang ky');
        END IF;

        SELECT COUNT(*)
        INTO v_count
        FROM LichThi
        WHERE kyThiId = p_kyThiId;

        IF v_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'Ky thi da co lich thi');
        END IF;

        DELETE FROM KyThi
        WHERE kyThiId = p_kyThiId;
    END;

    PROCEDURE GET_LICHTHI_BY_KYTHI(
        p_kyThiId NUMBER,
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT
            lichThiId,
            thoiGianThi,
            diaDiem,
            kyThiId
        FROM LichThi
        WHERE kyThiId = p_kyThiId
        ORDER BY lichThiId;
    END;

    PROCEDURE AUTO_PHANCONG_LICHTHI(
        p_kyThiId NUMBER
    )
    IS
        v_total NUMBER := 0;
        v_scheduleCount NUMBER := 0;
        v_base NUMBER := 0;
        v_extra NUMBER := 0;
        v_currentSchedule NUMBER := 1;
        v_currentLimit NUMBER := 0;
        v_currentCount NUMBER := 0;
        v_scheduleIds T_NUMBER_TAB;
        v_existLichThi NUMBER := 0;
    BEGIN
        SELECT COUNT(*)
        INTO v_existLichThi
        FROM LichThi
        WHERE kyThiId = p_kyThiId;

        IF v_existLichThi > 0 THEN
            RAISE_APPLICATION_ERROR(-20004, 'Ky thi da duoc phan cong lich thi');
        END IF;

        SELECT COUNT(*)
        INTO v_total
        FROM VW_KYTHI_HOCVIEN_DANGKY
        WHERE kyThiId = p_kyThiId;

        IF v_total = 0 THEN
            RAISE_APPLICATION_ERROR(-20005, 'Khong co hoc vien hop le de phan cong');
        END IF;

        v_scheduleCount := CEIL(v_total / 50);
        v_base := FLOOR(v_total / v_scheduleCount);
        v_extra := MOD(v_total, v_scheduleCount);

        FOR i IN 1 .. v_scheduleCount LOOP
            INSERT INTO LichThi (thoiGianThi, diaDiem, kyThiId)
            VALUES (NULL, NULL, p_kyThiId)
            RETURNING lichThiId INTO v_scheduleIds(i);
        END LOOP;

        v_currentSchedule := 1;
        v_currentLimit := v_base + CASE WHEN v_currentSchedule <= v_extra THEN 1 ELSE 0 END;
        v_currentCount := 0;

        FOR rec IN (
            SELECT hoSoId
            FROM VW_KYTHI_HOCVIEN_DANGKY
            WHERE kyThiId = p_kyThiId
            ORDER BY hoSoId
        ) LOOP

            IF v_currentCount >= v_currentLimit THEN
                v_currentSchedule := v_currentSchedule + 1;
                v_currentCount := 0;
                v_currentLimit := v_base + CASE WHEN v_currentSchedule <= v_extra THEN 1 ELSE 0 END;
            END IF;

            UPDATE ChiTietDangKyThi
            SET lichThiId = v_scheduleIds(v_currentSchedule)
            WHERE kyThiId = p_kyThiId
              AND hoSoId = rec.hoSoId;

            v_currentCount := v_currentCount + 1;
        END LOOP;
    END;

    PROCEDURE GET_HOCVIEN_DANGKY(
        p_kyThiId NUMBER,
        p_keyword NVARCHAR2,
        p_page NUMBER,
        p_pageSize NUMBER,
        p_cursor OUT SYS_REFCURSOR
    )
    IS
        v_start NUMBER;
        v_end NUMBER;
    BEGIN
        v_start := ((p_page - 1) * p_pageSize) + 1;
        v_end := p_page * p_pageSize;

        OPEN p_cursor FOR
        SELECT *
        FROM
        (
            SELECT x.*, ROWNUM rn
            FROM
            (
                SELECT
                    kyThiId,
                    tenKyThi,
                    hoSoId,
                    hocVienId,
                    hoTen,
                    sdt,
                    email,
                    lichThiId,
                    phieuId,
                    tenPhieu,
                    ngayLap,
                    ngayNop,
                    phuongThuc
                FROM VW_KYTHI_HOCVIEN_DANGKY
                WHERE kyThiId = p_kyThiId
                  AND (
                        p_keyword IS NULL
                        OR UPPER(hoTen) LIKE '%' || UPPER(p_keyword) || '%'
                        OR UPPER(sdt) LIKE '%' || UPPER(p_keyword) || '%'
                        OR UPPER(email) LIKE '%' || UPPER(p_keyword) || '%'
                      )
                ORDER BY hoTen, hoSoId
            ) x
            WHERE ROWNUM <= v_end
        )
        WHERE rn >= v_start;
    END;

    PROCEDURE GET_HOCVIEN_DANGKY_COUNT(
        p_kyThiId NUMBER,
        p_keyword NVARCHAR2,
        p_total OUT NUMBER
    )
    IS
    BEGIN
        SELECT COUNT(*)
        INTO p_total
        FROM VW_KYTHI_HOCVIEN_DANGKY
        WHERE kyThiId = p_kyThiId
          AND (
                p_keyword IS NULL
                OR UPPER(hoTen) LIKE '%' || UPPER(p_keyword) || '%'
                OR UPPER(sdt) LIKE '%' || UPPER(p_keyword) || '%'
                OR UPPER(email) LIKE '%' || UPPER(p_keyword) || '%'
              );
    END;
    
    PROCEDURE GET_KYTHI_EDIT_INFO(
        p_kyThiId NUMBER,
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT
            kt.kyThiId,
            hg.hangId,
            hg.maHang,
            hg.tenHang,
            TO_NUMBER(REGEXP_SUBSTR(kt.tenKyThi, 'đợt ([0-9]+)', 1, 1, NULL, 1)) AS dot,
            TO_NUMBER(REGEXP_SUBSTR(kt.tenKyThi, 'năm ([0-9]+)', 1, 1, NULL, 1)) AS nam
        FROM KyThi kt
        JOIN HangGplx hg
            ON hg.maHang = REGEXP_SUBSTR(kt.tenKyThi, '\(([^)]+)\)$', 1, 1, NULL, 1)
        WHERE kt.kyThiId = p_kyThiId;
    END;
    
    PROCEDURE UPDATE_CAP_KYTHI(
        p_kyThiId NUMBER,
        p_hangId NUMBER,
        p_dot NUMBER,
        p_nam NUMBER
    )
    IS
        v_oldTenKyThi KyThi.tenKyThi%TYPE;
        v_oldMaHang HangGplx.maHang%TYPE;
        v_oldDot NUMBER;
        v_oldNam NUMBER;
    
        v_newTenHang HangGplx.tenHang%TYPE;
        v_newMaHang HangGplx.maHang%TYPE;
    
        v_tenTotNghiep NVARCHAR2(255);
        v_tenSatHach NVARCHAR2(255);
    
        v_count NUMBER;
    BEGIN
        SELECT tenKyThi
        INTO v_oldTenKyThi
        FROM KyThi
        WHERE kyThiId = p_kyThiId;
    
        v_oldMaHang := REGEXP_SUBSTR(v_oldTenKyThi, '\(([^)]+)\)$', 1, 1, NULL, 1);
        v_oldDot := TO_NUMBER(REGEXP_SUBSTR(v_oldTenKyThi, 'đợt ([0-9]+)', 1, 1, NULL, 1));
        v_oldNam := TO_NUMBER(REGEXP_SUBSTR(v_oldTenKyThi, 'năm ([0-9]+)', 1, 1, NULL, 1));
    
        SELECT tenHang, maHang
        INTO v_newTenHang, v_newMaHang
        FROM HangGplx
        WHERE hangId = p_hangId;
    
        v_tenTotNghiep := 'Kỳ thi hạng ' || v_newTenHang || ' - Tốt nghiệp - đợt ' || p_dot || ' năm ' || p_nam || ' (' || v_newMaHang || ')';
        v_tenSatHach := 'Kỳ thi hạng ' || v_newTenHang || ' - Sát hạch - đợt ' || p_dot || ' năm ' || p_nam || ' (' || v_newMaHang || ')';
    
        SELECT COUNT(*)
        INTO v_count
        FROM KyThi
        WHERE tenKyThi IN (v_tenTotNghiep, v_tenSatHach)
          AND NOT (
                REGEXP_SUBSTR(tenKyThi, '\(([^)]+)\)$', 1, 1, NULL, 1) = v_oldMaHang
            AND TO_NUMBER(REGEXP_SUBSTR(tenKyThi, 'đợt ([0-9]+)', 1, 1, NULL, 1)) = v_oldDot
            AND TO_NUMBER(REGEXP_SUBSTR(tenKyThi, 'năm ([0-9]+)', 1, 1, NULL, 1)) = v_oldNam
          );
    
        IF v_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20006, 'Cap ky thi moi da ton tai');
        END IF;
    
        UPDATE KyThi
        SET tenKyThi =
            CASE
                WHEN loaiKyThi = N'Tốt nghiệp' THEN v_tenTotNghiep
                WHEN loaiKyThi = N'Sát hạch' THEN v_tenSatHach
                ELSE tenKyThi
            END
        WHERE REGEXP_SUBSTR(tenKyThi, '\(([^)]+)\)$', 1, 1, NULL, 1) = v_oldMaHang
          AND TO_NUMBER(REGEXP_SUBSTR(tenKyThi, 'đợt ([0-9]+)', 1, 1, NULL, 1)) = v_oldDot
          AND TO_NUMBER(REGEXP_SUBSTR(tenKyThi, 'năm ([0-9]+)', 1, 1, NULL, 1)) = v_oldNam
          AND loaiKyThi IN (N'Tốt nghiệp', N'Sát hạch');
    
        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20007, 'Khong tim thay cap ky thi can sua');
        END IF;
    END;
END PKG_KYTHI;
/
--CREATE OR REPLACE PROCEDURE SP_CREATE_KYTHI
--(
--    p_tenKyThi   KyThi.tenKyThi%TYPE,
--    p_loaiKyThi  KyThi.loaiKyThi%TYPE
--)
--AS
--BEGIN
--    INSERT INTO KyThi (tenKyThi, loaiKyThi)
--    VALUES (p_tenKyThi, p_loaiKyThi);
--END;
--/
--
--CREATE OR REPLACE PROCEDURE SP_UPDATE_KYTHI
--(
--    p_kyThiId NUMBER,
--    p_tenKyThi NVARCHAR2,
--    p_loaiKyThi NVARCHAR2
--)
--AS
--BEGIN
--    UPDATE KyThi
--    SET 
--        tenKyThi = p_tenKyThi,
--        loaiKyThi = p_loaiKyThi
--    WHERE kyThiId = p_kyThiId;
--END;
--/
--CREATE OR REPLACE PROCEDURE SP_DELETE_KYTHI
--(
--    p_kyThiId NUMBER
--)
--AS
--BEGIN
--    DELETE FROM KyThi
--    WHERE kyThiId = p_kyThiId;
--END;
--/
--CREATE OR REPLACE PROCEDURE SP_GET_KYTHI
--(
--    p_cursor OUT SYS_REFCURSOR
--)
--AS
--BEGIN
--    OPEN p_cursor FOR
--    SELECT * FROM VW_KYTHI_ADMIN
--    ORDER BY kyThiId DESC;
--END;
--/
--CREATE OR REPLACE PROCEDURE SP_CREATE_LICHTHI
--(
--    p_kyThiId NUMBER,
--    p_thoiGianThi TIMESTAMP,
--    p_diaDiem NVARCHAR2
--)
--AS
--BEGIN
--    INSERT INTO LichThi (kyThiId, thoiGianThi, diaDiem)
--    VALUES (p_kyThiId, p_thoiGianThi, p_diaDiem);
--END;
--/
--CREATE OR REPLACE PROCEDURE SP_GET_LICHTHI_BY_KYTHI
--(
--    p_kyThiId NUMBER,
--    p_cursor OUT SYS_REFCURSOR
--)
--AS
--BEGIN
--    OPEN p_cursor FOR
--    SELECT *
--    FROM LichThi
--    WHERE kyThiId = p_kyThiId
--    ORDER BY thoiGianThi;
--END;
--/



-- ============================================================
-- ====================        USER        ====================
-- ============================================================
-- HOME
CREATE OR REPLACE PROCEDURE SP_HOME_DASHBOARD (
    p_user_id              IN NUMBER,
    p_total_courses        OUT NUMBER,
    p_total_exams          OUT NUMBER,
    p_total_licenses       OUT NUMBER,
    p_featured_courses     OUT SYS_REFCURSOR,
    p_my_profiles          OUT SYS_REFCURSOR,
    p_my_exam_results      OUT SYS_REFCURSOR
)
AS
BEGIN
    /* ================= TỔNG QUAN CHUNG ================= */

    SELECT COUNT(*)
    INTO p_total_courses
    FROM KhoaHoc;

    SELECT COUNT(*)
    INTO p_total_exams
    FROM KyThi;

    SELECT COUNT(*)
    INTO p_total_licenses
    FROM GiayPhepLaiXe;

    /* ================= KHÓA HỌC NỔI BẬT ================= */

    OPEN p_featured_courses FOR
        SELECT
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON hg.hangId = kh.hangId
        ORDER BY kh.ngayBatDau DESC
        FETCH FIRST 6 ROWS ONLY;

    /* ================= NẾU CHƯA ĐĂNG NHẬP ================= */

    IF p_user_id IS NULL OR p_user_id = 0 THEN

        OPEN p_my_profiles FOR
            SELECT
                CAST(NULL AS NUMBER) AS hoSoId,
                CAST(NULL AS NVARCHAR2(100)) AS tenHoSo,
                CAST(NULL AS NVARCHAR2(20)) AS tenHang,
                CAST(NULL AS DATE) AS ngayDangKy,
                CAST(NULL AS NVARCHAR2(50)) AS trangThai
            FROM dual
            WHERE 1 = 0;

        OPEN p_my_exam_results FOR
            SELECT
                CAST(NULL AS NVARCHAR2(100)) AS tenBaiThi,
                CAST(NULL AS NVARCHAR2(50)) AS ketQuaDatDuoc,
                CAST(NULL AS BINARY_FLOAT) AS tongDiem,
                CAST(NULL AS NVARCHAR2(100)) AS tenKyThi
            FROM dual
            WHERE 1 = 0;

    ELSE

        /* ================= HỒ SƠ CỦA USER ================= */

        OPEN p_my_profiles FOR
            SELECT
                hs.hoSoId,
                hs.tenHoSo,
                hg.tenHang,
                hs.ngayDangKy,
                hs.trangThai
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN HangGplx hg ON hg.hangId = hs.hangId
            WHERE hv.userId = p_user_id
            ORDER BY hs.ngayDangKy DESC;

        /* ================= KẾT QUẢ THI CỦA USER ================= */

        OPEN p_my_exam_results FOR
            SELECT
                bt.tenBaiThi,
                ct.ketQuaDatDuoc,
                ct.tongDiem,
                kt.tenKyThi
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN ChiTietKetQuaThi ct ON ct.hoSoId = hs.hoSoId
            JOIN BaiThi bt ON bt.baiThiId = ct.baiThiId
            JOIN KyThi kt ON kt.kyThiId = bt.kyThiId
            WHERE hv.userId = p_user_id
            ORDER BY bt.baiThiId DESC
            FETCH FIRST 5 ROWS ONLY;

    END IF;

END;
/

-- KHÓA HỌC
CREATE OR REPLACE TRIGGER trg_khoahoc_trangthai
BEFORE INSERT OR UPDATE ON KhoaHoc
FOR EACH ROW
BEGIN
    :NEW.trangThai :=
        CASE
            WHEN TRUNC(SYSDATE) < TRUNC(:NEW.ngayBatDau)
                THEN N'Sắp khai giảng'
            WHEN TRUNC(SYSDATE) BETWEEN TRUNC(:NEW.ngayBatDau) AND TRUNC(:NEW.ngayKetThuc)
                THEN N'Đang học'
            WHEN TRUNC(SYSDATE) > TRUNC(:NEW.ngayKetThuc)
                THEN N'Đã kết thúc'
        END;
END;
/
CREATE OR REPLACE PACKAGE pkg_khoahoc AS
    PROCEDURE CAP_NHAT_TRANG_THAI;
END pkg_khoahoc;
/
CREATE OR REPLACE PACKAGE BODY pkg_khoahoc AS

    PROCEDURE CAP_NHAT_TRANG_THAI IS
    BEGIN
        UPDATE KhoaHoc
        SET trangThai =
            CASE
                WHEN TRUNC(SYSDATE) < TRUNC(ngayBatDau)
                    THEN N'Sắp khai giảng'
                WHEN TRUNC(SYSDATE) BETWEEN TRUNC(ngayBatDau) AND TRUNC(ngayKetThuc)
                    THEN N'Đang học'
                WHEN TRUNC(SYSDATE) > TRUNC(ngayKetThuc)
                    THEN N'Đã kết thúc'
            END;

        COMMIT;
    END CAP_NHAT_TRANG_THAI;

END pkg_khoahoc;
/
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDangMo (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.tenHang,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.trangThai IN (N'Sắp khai giảng', N'Đang học')
        ORDER BY kh.ngayBatDau ASC;
END;
/
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDetail (
    p_khoaHocId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hangId,
            hg.tenHang,
            hg.moTa,
            hg.loaiPhuongTien,
            hg.soCauHoi,
            hg.diemDat,
            hg.thoiGianTn,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.khoaHocId = p_khoaHocId;
END;
/
-- HỌC DASHBOASRD
CREATE OR REPLACE PROCEDURE PR_HOC_INDEX (
    P_MAHANG   IN VARCHAR2,
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG   VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH HANG_INFO AS (
            SELECT
                HG.HANGID,
                TRIM(UPPER(HG.MAHANG)) AS MAHANG,
                HG.THOIGIANTN,
                HG.SOCAUHOI
            FROM HANGGPLX HG
            WHERE TRIM(UPPER(HG.MAHANG)) = V_MAHANG
        ),
        FLAG_INFO AS (
            SELECT
                CASE
                    WHEN V_MAHANG IN ('A', 'A1') THEN 1
                    ELSE 0
                END AS IS_XEMAY
            FROM DUAL
        )
        SELECT
            H.HANGID,
            H.MAHANG,
            H.THOIGIANTN,
            (H.THOIGIANTN / 60) AS THOIGIANTHI,
            H.SOCAUHOI AS SOCAUTHINGAUNHIEN,

            (
                SELECT COUNT(*)
                FROM BODETHITHU B
                WHERE B.HANGID = H.HANGID
                  AND B.HOATDONG = 1
            ) AS TOTAL_BODE,

            CASE
                WHEN P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAM BL
                    JOIN BODETHITHU B ON B.BODEID = BL.BODEID
                    WHERE BL.USERID = P_USERID
                      AND B.HANGID = H.HANGID
                )
            END AS DONE_BODE,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUHOI,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CAULIET = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAULIET,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CHUY = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUCHUY,

            (
                SELECT COUNT(*)
                FROM BIENBAO
            ) AS TOTAL_BIENBAO,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE 1
            END AS HAS_MOPHONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BODEMOPHONG BMP
                    WHERE BMP.ISACTIVE = 1
                )
            END AS MP_BODE,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM TINHHUONGMOPHONG
                )
            END AS MP_TINHHUONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') OR P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAMMOPHONG BLM
                    JOIN BODEMOPHONG BMP ON BMP.BODEMOPHONGID = BLM.BODEMOPHONGID
                    WHERE BLM.USERID = P_USERID
                      AND BMP.ISACTIVE = 1
                )
            END AS MP_BODE_DONE
        FROM HANG_INFO H;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CAU_LIET (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CAULIET = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CHU_Y (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CHUY = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_BIENBAO (
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            B.IDBIENBAO,
            B.TENBIENBAO,
            B.YNGHIA,
            B.HINHANH,
            F.IDFLASHCARD,
            F.DANHGIA
        FROM BIENBAO B
        LEFT JOIN FLASHCARD F
               ON F.IDBIENBAO = B.IDBIENBAO
              AND F.USERID = P_USERID
        ORDER BY B.IDBIENBAO;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_SAVE (
    P_USERID      IN NUMBER,
    P_IDBIENBAO   IN NUMBER,
    P_DANHGIA     IN NVARCHAR2
)
AS
    V_COUNT NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO V_COUNT
    FROM FLASHCARD
    WHERE USERID = P_USERID
      AND IDBIENBAO = P_IDBIENBAO;

    IF V_COUNT = 0 THEN
        INSERT INTO FLASHCARD (DANHGIA, USERID, IDBIENBAO)
        VALUES (P_DANHGIA, P_USERID, P_IDBIENBAO);
    ELSE
        UPDATE FLASHCARD
        SET DANHGIA = P_DANHGIA
        WHERE USERID = P_USERID
          AND IDBIENBAO = P_IDBIENBAO;
    END IF;
END;
/
CREATE OR REPLACE PROCEDURE PR_GET_HANG
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT MAHANG FROM HANGGPLX ORDER BY MAHANG;
END;
/
-- LÝ THUYẾT
CREATE OR REPLACE VIEW VW_BODE_LYTHUYET AS
SELECT 
    B.BODEID,
    B.TENBODE,
    NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
    NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
    B.HANGID,
    H.MAHANG,
    H.DIEMDAT,
    H.THOIGIANTN
FROM BODETHITHU B
JOIN HANGGPLX H ON H.HANGID = B.HANGID
WHERE B.HOATDONG = 1;
/

CREATE OR REPLACE PROCEDURE PR_GET_BODE_BY_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT *
    FROM VW_BODE_LYTHUYET
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG))
    ORDER BY BODEID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_EXAM_DETAIL
(
    P_BODEID IN NUMBER,
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        B.BODEID,
        B.TENBODE,
        NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
        NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
        H.DIEMDAT,
        TRIM(H.MAHANG) AS MAHANG,
        CH.CAUHOIID,
        CH.NOIDUNG,
        CH.HINHANH,
        CH.URLANHMEO,
        NVL(CH.CAULIET, 0) AS CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM BODETHITHU B
    JOIN HANGGPLX H ON H.HANGID = B.HANGID
    JOIN CHITIETBODETRACNGHIEM CT ON CT.BODEID = B.BODEID
    JOIN CAUHOILYTHUYET CH ON CH.CAUHOIID = CT.CAUHOIID
    JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
    WHERE B.BODEID = P_BODEID
      AND B.HOATDONG = 1
      AND UPPER(TRIM(H.MAHANG)) = UPPER(TRIM(P_MAHANG))
    ORDER BY NVL(CT.THUTU, 999999), CH.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HISTORY_EXAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        BL.BAILAMID,
        BL.THOIGIANLAMBAI,
        BL.SOCAUSAI,
        NVL(BL.KETQUA, 0) AS KETQUA,
        CT.CAUHOIID,
        CT.DAPANDACHON,
        NVL(CT.KETQUACAU, 0) AS KETQUACAU
    FROM BAILAM BL
    JOIN CHITIETBAILAM CT ON CT.BAILAMID = BL.BAILAMID
    WHERE BL.BAILAMID = (
        SELECT MAX(BL2.BAILAMID)
        FROM BAILAM BL2
        WHERE BL2.USERID = P_USERID
          AND BL2.BODEID = P_BODEID
    )
    ORDER BY CT.CAUHOIID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_LAST_BAILAM_BY_HANG
(
    P_USERID IN NUMBER,
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        X.BODEID,
        X.BAILAMID,
        X.THOIGIANLAMBAI,
        X.SOCAUSAI,
        X.KETQUA
    FROM
    (
        SELECT
            BL.BODEID,
            BL.BAILAMID,
            BL.THOIGIANLAMBAI,
            BL.SOCAUSAI,
            NVL(BL.KETQUA, 0) AS KETQUA,
            ROW_NUMBER() OVER (PARTITION BY BL.BODEID ORDER BY BL.BAILAMID DESC) AS RN
        FROM BAILAM BL
        JOIN BODETHITHU B ON B.BODEID = BL.BODEID
        JOIN HANGGPLX H ON H.HANGID = B.HANGID
        WHERE BL.USERID = P_USERID
          AND UPPER(TRIM(H.MAHANG)) = UPPER(TRIM(P_MAHANG))
    ) X
    WHERE X.RN = 1;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HANG_INFO
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT
        HANGID,
        MAHANG,
        SOCAUHOI,
        DIEMDAT,
        THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_ALL_QUESTION_FOR_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_IS_XEMAY NUMBER := 0;
BEGIN
    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QUESTION_BASE AS
    (
        SELECT
            ROW_NUMBER() OVER (ORDER BY NVL(C.THUTU, 999999), CH.CAUHOIID) AS GLOBAL_INDEX,
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET,
            NVL(CH.CHUY, 0) AS CHUY,
            NVL(CH.XEMAY, 0) AS XEMAY,
            C.CHUONGID,
            C.TENCHUONG,
            NVL(C.THUTU, 999999) AS CHUONGTHUTU,
            D.DAPANID,
            D.THUTU AS DAPANTHUTU,
            NVL(D.DAPANDUNG, 0) AS DAPANDUNG
        FROM CAUHOILYTHUYET CH
        JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    )
    SELECT *
    FROM QUESTION_BASE
    ORDER BY CHUONGTHUTU, CAUHOIID, DAPANTHUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_RANDOM_EXAM
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_HANGID NUMBER;
    V_SOCAU NUMBER;
    V_DIEMDAT NUMBER;
    V_THOIGIANTN NUMBER;
    V_IS_XEMAY NUMBER := 0;
BEGIN
    SELECT HANGID, SOCAUHOI, DIEMDAT, THOIGIANTN
    INTO V_HANGID, V_SOCAU, V_DIEMDAT, V_THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));

    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QBASE AS
    (
        SELECT
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET
        FROM CAUHOILYTHUYET CH
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    ),
    RANDQ AS
    (
        SELECT *
        FROM
        (
            SELECT Q.*, DBMS_RANDOM.VALUE AS RND
            FROM QBASE Q
        )
        ORDER BY RND
        FETCH FIRST V_SOCAU ROWS ONLY
    )
    SELECT
        -1 AS BODEID,
        N'Đề thi ngẫu nhiên hạng ' || P_MAHANG AS TENBODE,
        V_THOIGIANTN AS THOIGIAN,
        V_SOCAU AS SOCAUHOI,
        V_DIEMDAT AS DIEMDAT,
        P_MAHANG AS MAHANG,
        R.CAUHOIID,
        R.NOIDUNG,
        R.HINHANH,
        R.URLANHMEO,
        R.CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM RANDQ R
    JOIN DAPAN D ON D.CAUHOIID = R.CAUHOIID
    ORDER BY R.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_BAILAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_THOIGIAN IN NUMBER,
    P_SOCAUSAI IN NUMBER,
    P_KETQUA IN NUMBER,
    P_BAILAMID OUT NUMBER
)
AS
BEGIN
    INSERT INTO BAILAM(USERID, BODEID, THOIGIANLAMBAI, SOCAUSAI, KETQUA)
    VALUES (P_USERID, P_BODEID, P_THOIGIAN, P_SOCAUSAI, P_KETQUA)
    RETURNING BAILAMID INTO P_BAILAMID;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_CT_BAILAM
(
    P_BAILAMID IN NUMBER,
    P_CAUHOIID IN NUMBER,
    P_DAPAN IN NVARCHAR2,
    P_KETQUA IN NUMBER
)
AS
BEGIN
    INSERT INTO CHITIETBAILAM(BAILAMID, CAUHOIID, DAPANDACHON, KETQUACAU)
    VALUES (P_BAILAMID, P_CAUHOIID, P_DAPAN, P_KETQUA);
END;
/

CREATE OR REPLACE FUNCTION FUNC_CHAM_DIEM
(
    P_BAILAMID IN NUMBER
)
RETURN NUMBER
AS
    V_DUNG NUMBER := 0;
BEGIN
    SELECT COUNT(*)
    INTO V_DUNG
    FROM CHITIETBAILAM
    WHERE BAILAMID = P_BAILAMID
      AND KETQUACAU = 1;

    RETURN V_DUNG;
END;
/
CREATE OR REPLACE PROCEDURE PR_GET_RANDOM_EXAM_BY_IDS
(
    P_MAHANG IN VARCHAR2,
    P_IDS IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_DIEMDAT NUMBER;
    V_THOIGIANTN NUMBER;
BEGIN
    SELECT DIEMDAT, THOIGIANTN
    INTO V_DIEMDAT, V_THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
    WITH ID_LIST AS
    (
        SELECT TO_NUMBER(REGEXP_SUBSTR(P_IDS, '[^,]+', 1, LEVEL)) AS CAUHOIID
        FROM DUAL
        CONNECT BY REGEXP_SUBSTR(P_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
    )
    SELECT
        -1 AS BODEID,
        N'Đề thi ngẫu nhiên hạng ' || P_MAHANG AS TENBODE,
        V_THOIGIANTN AS THOIGIAN,
        (SELECT COUNT(*) FROM ID_LIST) AS SOCAUHOI,
        V_DIEMDAT AS DIEMDAT,
        P_MAHANG AS MAHANG,
        CH.CAUHOIID,
        CH.NOIDUNG,
        CH.HINHANH,
        CH.URLANHMEO,
        NVL(CH.CAULIET, 0) AS CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM ID_LIST I
    JOIN CAUHOILYTHUYET CH ON CH.CAUHOIID = I.CAUHOIID
    JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
    ORDER BY I.CAUHOIID, D.THUTU;
END;
/
/* =========================================================
   THI MÔ PHỎNG - OBJECT ORACLE
   Dùng cho:
   - DanhSachBoDe
   - LichSuBaiLam
   - LamBai
   - LuuKetQua
   - KetQua
   - LamBaiNgauNhien
   - LuuKetQuaNgauNhien
========================================================= */

------------------------------------------------------------
-- 1. TYPE PHỤ TRỢ CHO ĐỀ NGẪU NHIÊN
------------------------------------------------------------
CREATE OR REPLACE TYPE OBJ_MP_RANDOM_ROW FORCE AS OBJECT
(
    IDTHMP          NUMBER,
    TIEUDE          NVARCHAR2(255),
    VIDEOURL        NVARCHAR2(1000),
    SCORESTARTSEC   NUMBER,
    SCOREENDSEC     NUMBER,
    HINTIMAGEURL    NVARCHAR2(500),
    KHO             NUMBER
);
/
CREATE OR REPLACE TYPE TAB_MP_RANDOM_ROW FORCE AS TABLE OF OBJ_MP_RANDOM_ROW;
/

------------------------------------------------------------
-- 2. VIEW
------------------------------------------------------------
CREATE OR REPLACE VIEW VW_MP_BODE_TINHHUONG AS
SELECT
    CAST(B.BODEMOPHONGID AS NUMBER(10)) AS BODEMOPHONGID,
    CAST(NVL(B.TENBODE, '') AS NVARCHAR2(255)) AS TENBODE,
    CAST(NVL(B.SOTINHHUONG, 10) AS NUMBER(10)) AS SOTINHHUONG,
    CAST(NVL(B.ISACTIVE, 0) AS NUMBER(1)) AS ISACTIVE,
    CAST(NVL(CT.THUTU, 0) AS NUMBER(10)) AS THUTU,
    CAST(TH.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
    CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
    CAST(NVL(TH.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
    CAST(NVL(TH.URLANHMEO, '') AS NVARCHAR2(500)) AS URLANHMEO,
    CAST(NVL(TH.KHO, 0) AS NUMBER(1)) AS KHO,
    CAST(NVL(TH.TGBATDAU, 0) AS NUMBER(12,4)) AS TGBATDAU,
    CAST(NVL(TH.TGKETTHUC, 0) AS NUMBER(12,4)) AS TGKETTHUC,
    CAST(TH.CHUONGMOPHONGID AS NUMBER(10)) AS CHUONGMOPHONGID
FROM BODEMOPHONG B
JOIN CHITIETBODEMOPHONG CT
    ON CT.BODEMOPHONGID = B.BODEMOPHONGID
JOIN TINHHUONGMOPHONG TH
    ON TH.TINHHUONGMOPHONGID = CT.TINHHUONGMOPHONGID;
/

CREATE OR REPLACE VIEW VW_MP_BAILAM_CHITIET AS
SELECT
    BL.BAILAMMOPHONGID,
    BL.USERID,
    BL.BODEMOPHONGID,
    BL.TONGDIEM,
    BL.KETQUA,
    D.TINHHUONGMOPHONGID AS IDTHMP,
    D.THOIDIEMNGUOIDUNGNHAN,
    TH.TIEUDE,
    TH.VIDEOURL,
    TH.URLANHMEO,
    TH.KHO,
    TH.TGBATDAU,
    TH.TGKETTHUC
FROM BAILAMMOPHONG BL
LEFT JOIN DIEMTUNGTINHHUONG D
    ON D.BAILAMMOPHONGID = BL.BAILAMMOPHONGID
LEFT JOIN TINHHUONGMOPHONG TH
    ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID;
/

------------------------------------------------------------
-- 3. FUNCTION
------------------------------------------------------------
CREATE OR REPLACE FUNCTION FUNC_MP_FRAME_TO_SEC
(
    P_FRAME IN NUMBER
)
RETURN NUMBER
AS
BEGIN
    RETURN NVL(P_FRAME, 0) / 60;
END;
/

CREATE OR REPLACE FUNCTION FUNC_MP_TINH_DIEM
(
    P_TIMEPRESSSEC IN NUMBER,
    P_STARTFRAME   IN NUMBER,
    P_ENDFRAME     IN NUMBER
)
RETURN NUMBER
AS
    V_STARTSEC NUMBER;
    V_ENDSEC   NUMBER;
    V_DURATION NUMBER;
    V_STEP     NUMBER;
    V_INDEX    NUMBER;
BEGIN
    V_STARTSEC := FUNC_MP_FRAME_TO_SEC(P_STARTFRAME);
    V_ENDSEC   := FUNC_MP_FRAME_TO_SEC(P_ENDFRAME);

    IF V_ENDSEC <= V_STARTSEC THEN
        RETURN 0;
    END IF;

    IF NVL(P_TIMEPRESSSEC, 0) < V_STARTSEC OR NVL(P_TIMEPRESSSEC, 0) > V_ENDSEC THEN
        RETURN 0;
    END IF;

    V_DURATION := V_ENDSEC - V_STARTSEC;
    V_STEP     := V_DURATION / 5;

    V_INDEX := FLOOR((P_TIMEPRESSSEC - V_STARTSEC) / V_STEP);

    IF V_INDEX < 0 THEN
        V_INDEX := 0;
    ELSIF V_INDEX > 4 THEN
        V_INDEX := 4;
    END IF;

    RETURN 5 - V_INDEX;
END;
/

CREATE OR REPLACE FUNCTION FUNC_MP_GET_FLAG_TIME
(
    P_FLAGS_TEXT IN CLOB,
    P_IDTHMP     IN NUMBER
)
RETURN NUMBER
AS
    V_TOKEN   VARCHAR2(4000);
    V_ID      NUMBER;
    V_TIME    NUMBER;
    V_IDX     NUMBER := 1;
    V_MIN     NUMBER := NULL;
BEGIN
    LOOP
        V_TOKEN := REGEXP_SUBSTR(P_FLAGS_TEXT, '[^;]+', 1, V_IDX);
        EXIT WHEN V_TOKEN IS NULL;

        V_ID := TO_NUMBER(REGEXP_SUBSTR(V_TOKEN, '^[^|]+'));

        IF V_ID = P_IDTHMP THEN
            V_TIME := TO_NUMBER(
                REGEXP_SUBSTR(V_TOKEN, '[^|]+', 1, 2),
                '9999999990D999999',
                'NLS_NUMERIC_CHARACTERS = ''.,'''
            );

            IF V_MIN IS NULL OR V_TIME < V_MIN THEN
                V_MIN := V_TIME;
            END IF;
        END IF;

        V_IDX := V_IDX + 1;
    END LOOP;

    RETURN V_MIN;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
/

------------------------------------------------------------
-- 4. PROCEDURE - DANH SÁCH BỘ ĐỀ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_DANH_SACH_BODE
(
    P_USERID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        WITH LATEST_BAILAM AS
        (
            SELECT *
            FROM
            (
                SELECT
                    BL.*,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY BL.BODEMOPHONGID
                        ORDER BY BL.BAILAMMOPHONGID DESC
                    ) AS RN
                FROM BAILAMMOPHONG BL
                WHERE BL.USERID = P_USERID
            )
            WHERE RN = 1
        )
        SELECT
            B.BODEMOPHONGID AS IDBODE,
            B.TENBODE,
            NVL(B.SOTINHHUONG, 10) AS SOTINHHUONG,
            CASE
                WHEN LB.BAILAMMOPHONGID IS NULL THEN 0
                ELSE 1
            END AS HASRESULT,
            NVL(LB.TONGDIEM, 0) AS TONGDIEM,
            NVL(LB.KETQUA, 0) AS KETQUA,
            CASE
                WHEN LB.BAILAMMOPHONGID IS NULL THEN 0
                ELSE
                    (
                        SELECT COUNT(*)
                        FROM DIEMTUNGTINHHUONG D
                        JOIN TINHHUONGMOPHONG TH
                            ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID
                        WHERE D.BAILAMMOPHONGID = LB.BAILAMMOPHONGID
                          AND
                          (
                              D.THOIDIEMNGUOIDUNGNHAN < FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU)
                              OR D.THOIDIEMNGUOIDUNGNHAN > FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC)
                          )
                    )
            END AS SOTINHHUONGSAI,
            LB.BAILAMMOPHONGID AS IDBAILAMMOINHAT
        FROM BODEMOPHONG B
        LEFT JOIN LATEST_BAILAM LB
            ON LB.BODEMOPHONGID = B.BODEMOPHONGID
        WHERE B.ISACTIVE = 1
        ORDER BY B.BODEMOPHONGID;
END;
/

------------------------------------------------------------
-- 5. PROCEDURE - LÀM BÀI / CHI TIẾT BỘ ĐỀ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LAY_BODE_CHI_TIET
(
    P_IDBODE IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(V.BODEMOPHONGID AS NUMBER(10)) AS IDBODE,
            CAST(V.IDTHMP AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(V.THUTU, 0) AS NUMBER(10)) AS THUTU,
            CAST(NVL(V.SOTINHHUONG, 10) AS NUMBER(10)) AS SOTINHHUONG,

            CAST(V.TENBODE AS NVARCHAR2(255)) AS TENBODE,
            CAST(NVL(V.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(V.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(V.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,

            CAST(NVL(V.KHO, 0) AS NUMBER(1)) AS KHO,

            CAST(NVL(V.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(V.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU;
END;
/

------------------------------------------------------------
-- 6. PROCEDURE - LỊCH SỬ BÀI LÀM
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LICH_SU_HEADER
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            BL.BAILAMMOPHONGID AS IDBAILAM,
            BL.BODEMOPHONGID AS IDBODE,
            BL.TONGDIEM,
            NVL(BL.KETQUA, 0) AS KETQUA
        FROM BAILAMMOPHONG BL
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM;
END;
/

CREATE OR REPLACE PROCEDURE PROC_MP_LICH_SU_CHI_TIET
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(V.BODEMOPHONGID AS NUMBER(10)) AS IDBODE,
            CAST(NVL(V.THUTU, 0) AS NUMBER(10)) AS THUTU,
            CAST(V.IDTHMP AS NUMBER(10)) AS IDTHMP,

            CAST(NVL(V.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(V.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(V.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,

            CAST(NVL(V.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(V.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC,
            CAST(NVL(D.THOIDIEMNGUOIDUNGNHAN, 0) AS NUMBER(12,4)) AS TIMESEC
        FROM BAILAMMOPHONG BL
        JOIN VW_MP_BODE_TINHHUONG V
            ON V.BODEMOPHONGID = BL.BODEMOPHONGID
        LEFT JOIN DIEMTUNGTINHHUONG D
            ON D.BAILAMMOPHONGID = BL.BAILAMMOPHONGID
           AND D.TINHHUONGMOPHONGID = V.IDTHMP
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM
        ORDER BY V.THUTU;
END;
/

------------------------------------------------------------
-- 7. PROCEDURE - KẾT QUẢ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_KET_QUA_HEADER
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            BL.BAILAMMOPHONGID AS IDBAILAM,
            BL.TONGDIEM,
            NVL(BL.KETQUA, 0) AS KETQUA
        FROM BAILAMMOPHONG BL
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM;
END;
/

CREATE OR REPLACE PROCEDURE PROC_MP_KET_QUA_CHI_TIET
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(D.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(D.THOIDIEMNGUOIDUNGNHAN, 0) AS NUMBER(12,4)) AS THOIDIEMNHAN,
            CAST(
                FUNC_MP_TINH_DIEM(
                    D.THOIDIEMNGUOIDUNGNHAN,
                    TH.TGBATDAU,
                    TH.TGKETTHUC
                ) AS NUMBER(2)
            ) AS DIEM
        FROM DIEMTUNGTINHHUONG D
        JOIN TINHHUONGMOPHONG TH
            ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID
        WHERE D.BAILAMMOPHONGID = P_IDBAILAM
        ORDER BY D.TINHHUONGMOPHONGID;
END;
/

------------------------------------------------------------
-- 8. PROCEDURE - CHẤM ĐIỂM BỘ ĐỀ CỐ ĐỊNH (KHÔNG LƯU)
-- P_FLAGS_TEXT format: IDTHMP|TIMESEC;IDTHMP|TIMESEC;...
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_CHAM_BODE
(
    P_IDBODE     IN NUMBER,
    P_FLAGS_TEXT IN CLOB,
    O_TONGDIEM   OUT NUMBER,
    O_DAT        OUT NUMBER
)
AS
    V_TONG NUMBER := 0;
    V_TIME NUMBER;
BEGIN
    FOR R IN
    (
        SELECT
            V.IDTHMP,
            V.TGBATDAU,
            V.TGKETTHUC
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU
        FETCH FIRST 10 ROWS ONLY
    )
    LOOP
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, R.IDTHMP);
        V_TONG := V_TONG + FUNC_MP_TINH_DIEM(NVL(V_TIME, 0), R.TGBATDAU, R.TGKETTHUC);
    END LOOP;

    O_TONGDIEM := V_TONG;
    O_DAT := CASE WHEN V_TONG >= 35 THEN 1 ELSE 0 END;
END;
/

------------------------------------------------------------
-- 9. PROCEDURE - LƯU KẾT QUẢ BỘ ĐỀ CỐ ĐỊNH
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LUU_KET_QUA
(
    P_USERID      IN NUMBER,
    P_IDBODE      IN NUMBER,
    P_FLAGS_TEXT  IN CLOB,
    O_TONGDIEM    OUT NUMBER,
    O_DAT         OUT NUMBER,
    O_IDBAILAM    OUT NUMBER
)
AS
    V_TIME NUMBER;
BEGIN
    PROC_MP_CHAM_BODE
    (
        P_IDBODE     => P_IDBODE,
        P_FLAGS_TEXT => P_FLAGS_TEXT,
        O_TONGDIEM   => O_TONGDIEM,
        O_DAT        => O_DAT
    );

    INSERT INTO BAILAMMOPHONG
    (
        TONGDIEM,
        KETQUA,
        USERID,
        BODEMOPHONGID
    )
    VALUES
    (
        O_TONGDIEM,
        O_DAT,
        P_USERID,
        P_IDBODE
    )
    RETURNING BAILAMMOPHONGID INTO O_IDBAILAM;

    FOR R IN
    (
        SELECT
            V.IDTHMP
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU
        FETCH FIRST 10 ROWS ONLY
    )
    LOOP
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, R.IDTHMP);

        INSERT INTO DIEMTUNGTINHHUONG
        (
            BAILAMMOPHONGID,
            TINHHUONGMOPHONGID,
            THOIDIEMNGUOIDUNGNHAN
        )
        VALUES
        (
            O_IDBAILAM,
            R.IDTHMP,
            NVL(V_TIME, 0)
        );
    END LOOP;
END;
/

------------------------------------------------------------
-- 10. PROCEDURE - LẤY 10 TÌNH HUỐNG NGẪU NHIÊN
-- Tỷ lệ chương: 2-1-2-1-2-2 theo THUTU của CHUONGMOPHONG
-- Có 1 hoặc 2 câu khó
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_DE_NGAU_NHIEN
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    TYPE T_NUM_BY_IDX IS TABLE OF NUMBER INDEX BY PLS_INTEGER;

    V_Q1 NUMBER := 2;
    V_Q2 NUMBER := 1;
    V_Q3 NUMBER := 2;
    V_Q4 NUMBER := 1;
    V_Q5 NUMBER := 2;
    V_Q6 NUMBER := 2;

    V_HARD_COUNT NUMBER;
    V_HARD_NOW   NUMBER := 0;

    V_ROWS TAB_MP_RANDOM_ROW := TAB_MP_RANDOM_ROW();

    FUNCTION FN_HAS_ID(P_ID NUMBER) RETURN BOOLEAN
    AS
    BEGIN
        FOR I IN 1 .. V_ROWS.COUNT LOOP
            IF V_ROWS(I).IDTHMP = P_ID THEN
                RETURN TRUE;
            END IF;
        END LOOP;
        RETURN FALSE;
    END;

    PROCEDURE ADD_ROW
    (
        P_IDTHMP        IN NUMBER,
        P_TIEUDE        IN NVARCHAR2,
        P_VIDEOURL      IN NVARCHAR2,
        P_SCORESTARTSEC IN NUMBER,
        P_SCOREENDSEC   IN NUMBER,
        P_HINTIMAGEURL  IN NVARCHAR2,
        P_KHO           IN NUMBER
    )
    AS
    BEGIN
        IF FN_HAS_ID(P_IDTHMP) THEN
            RETURN;
        END IF;

        V_ROWS.EXTEND;
        V_ROWS(V_ROWS.COUNT) := OBJ_MP_RANDOM_ROW
        (
            P_IDTHMP,
            P_TIEUDE,
            P_VIDEOURL,
            P_SCORESTARTSEC,
            P_SCOREENDSEC,
            P_HINTIMAGEURL,
            P_KHO
        );
    END;

    FUNCTION GET_QUOTA(P_THUTU NUMBER) RETURN NUMBER
    AS
    BEGIN
        CASE P_THUTU
            WHEN 1 THEN RETURN V_Q1;
            WHEN 2 THEN RETURN V_Q2;
            WHEN 3 THEN RETURN V_Q3;
            WHEN 4 THEN RETURN V_Q4;
            WHEN 5 THEN RETURN V_Q5;
            WHEN 6 THEN RETURN V_Q6;
            ELSE RETURN 0;
        END CASE;
    END;

    PROCEDURE SET_QUOTA(P_THUTU NUMBER, P_VAL NUMBER)
    AS
    BEGIN
        CASE P_THUTU
            WHEN 1 THEN V_Q1 := P_VAL;
            WHEN 2 THEN V_Q2 := P_VAL;
            WHEN 3 THEN V_Q3 := P_VAL;
            WHEN 4 THEN V_Q4 := P_VAL;
            WHEN 5 THEN V_Q5 := P_VAL;
            WHEN 6 THEN V_Q6 := P_VAL;
        END CASE;
    END;
BEGIN
    V_HARD_COUNT := CASE WHEN DBMS_RANDOM.VALUE(1, 3) < 2 THEN 1 ELSE 2 END;

    -- B1. Pick hard trước theo quota chương
    FOR R IN
    (
        SELECT
            C.THUTU AS CHUONGTHUTU,
            TH.TINHHUONGMOPHONGID,
            TH.TIEUDE,
            TH.VIDEOURL,
            TH.URLANHMEO,
            TH.KHO,
            FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
            FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
        FROM CHUONGMOPHONG C
        JOIN TINHHUONGMOPHONG TH
            ON TH.CHUONGMOPHONGID = C.CHUONGMOPHONGID
        WHERE NVL(TH.KHO, 0) = 1
        ORDER BY DBMS_RANDOM.VALUE
    )
    LOOP
        EXIT WHEN V_HARD_NOW >= V_HARD_COUNT;

        IF GET_QUOTA(R.CHUONGTHUTU) > 0 THEN
            ADD_ROW
            (
                R.TINHHUONGMOPHONGID,
                R.TIEUDE,
                R.VIDEOURL,
                R.SCORESTARTSEC,
                R.SCOREENDSEC,
                R.URLANHMEO,
                NVL(R.KHO, 0)
            );

            SET_QUOTA(R.CHUONGTHUTU, GET_QUOTA(R.CHUONGTHUTU) - 1);
            V_HARD_NOW := V_HARD_NOW + 1;
        END IF;
    END LOOP;

    -- B2. Fill theo quota từng chương, ưu tiên câu thường
    FOR CH IN
    (
        SELECT CHUONGMOPHONGID, THUTU
        FROM CHUONGMOPHONG
        ORDER BY THUTU
    )
    LOOP
        IF GET_QUOTA(CH.THUTU) <= 0 THEN
            CONTINUE;
        END IF;

        FOR R IN
        (
            SELECT
                TH.TINHHUONGMOPHONGID,
                TH.TIEUDE,
                TH.VIDEOURL,
                TH.URLANHMEO,
                TH.KHO,
                FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
            FROM TINHHUONGMOPHONG TH
            WHERE TH.CHUONGMOPHONGID = CH.CHUONGMOPHONGID
              AND NVL(TH.KHO, 0) = 0
            ORDER BY DBMS_RANDOM.VALUE
        )
        LOOP
            EXIT WHEN GET_QUOTA(CH.THUTU) <= 0;

            ADD_ROW
            (
                R.TINHHUONGMOPHONGID,
                R.TIEUDE,
                R.VIDEOURL,
                R.SCORESTARTSEC,
                R.SCOREENDSEC,
                R.URLANHMEO,
                NVL(R.KHO, 0)
            );

            IF FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                SET_QUOTA(CH.THUTU, GET_QUOTA(CH.THUTU) - 1);
            END IF;
        END LOOP;

        IF GET_QUOTA(CH.THUTU) > 0 THEN
            FOR R IN
            (
                SELECT
                    TH.TINHHUONGMOPHONGID,
                    TH.TIEUDE,
                    TH.VIDEOURL,
                    TH.URLANHMEO,
                    TH.KHO,
                    FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                    FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
                FROM TINHHUONGMOPHONG TH
                WHERE TH.CHUONGMOPHONGID = CH.CHUONGMOPHONGID
                ORDER BY DBMS_RANDOM.VALUE
            )
            LOOP
                EXIT WHEN GET_QUOTA(CH.THUTU) <= 0;

                IF NOT FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                    ADD_ROW
                    (
                        R.TINHHUONGMOPHONGID,
                        R.TIEUDE,
                        R.VIDEOURL,
                        R.SCORESTARTSEC,
                        R.SCOREENDSEC,
                        R.URLANHMEO,
                        NVL(R.KHO, 0)
                    );
                    SET_QUOTA(CH.THUTU, GET_QUOTA(CH.THUTU) - 1);
                END IF;
            END LOOP;
        END IF;
    END LOOP;

    -- B3. Nếu thiếu thì fill toàn bộ hệ thống
    IF V_ROWS.COUNT < 10 THEN
        FOR R IN
        (
            SELECT
                TH.TINHHUONGMOPHONGID,
                TH.TIEUDE,
                TH.VIDEOURL,
                TH.URLANHMEO,
                TH.KHO,
                FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
            FROM TINHHUONGMOPHONG TH
            ORDER BY DBMS_RANDOM.VALUE
        )
        LOOP
            EXIT WHEN V_ROWS.COUNT >= 10;

            IF NOT FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                ADD_ROW
                (
                    R.TINHHUONGMOPHONGID,
                    R.TIEUDE,
                    R.VIDEOURL,
                    R.SCORESTARTSEC,
                    R.SCOREENDSEC,
                    R.URLANHMEO,
                    NVL(R.KHO, 0)
                );
            END IF;
        END LOOP;
    END IF;

    OPEN P_CURSOR FOR
    SELECT
        CAST(T.IDTHMP AS NUMBER(10)) AS IDTHMP,
        CAST(NVL(T.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
        CAST(NVL(T.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
        CAST(NVL(T.SCORESTARTSEC, 0) AS NUMBER(12,4)) AS SCORESTARTSEC,
        CAST(NVL(T.SCOREENDSEC, 0) AS NUMBER(12,4)) AS SCOREENDSEC,
        CAST(NVL(T.HINTIMAGEURL, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,
        CAST(NVL(T.KHO, 0) AS NUMBER(1)) AS KHO
    FROM
    (
        SELECT T.*
        FROM TABLE(V_ROWS) T
        ORDER BY DBMS_RANDOM.VALUE
    ) T
    FETCH FIRST 10 ROWS ONLY;
END;
/

------------------------------------------------------------
-- 11. PROCEDURE - CHẤM ĐỀ NGẪU NHIÊN (KHÔNG LƯU)
-- P_SELECTED_IDS format: 1,5,9,12...
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_CHAM_NGAU_NHIEN
(
    P_SELECTED_IDS IN CLOB,
    P_FLAGS_TEXT   IN CLOB,
    O_TONGDIEM     OUT NUMBER,
    O_DAT          OUT NUMBER
)
AS
    V_ID_STR VARCHAR2(100);
    V_ID     NUMBER;
    V_TIME   NUMBER;
    V_TONG   NUMBER := 0;
    V_IDX    NUMBER := 1;
    V_START  NUMBER;
    V_END    NUMBER;
BEGIN
    LOOP
        V_ID_STR := REGEXP_SUBSTR(P_SELECTED_IDS, '[^,]+', 1, V_IDX);
        EXIT WHEN V_ID_STR IS NULL;

        V_ID := TO_NUMBER(TRIM(V_ID_STR));
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, V_ID);

        SELECT TGBATDAU, TGKETTHUC
        INTO V_START, V_END
        FROM TINHHUONGMOPHONG
        WHERE TINHHUONGMOPHONGID = V_ID;

        V_TONG := V_TONG + FUNC_MP_TINH_DIEM(NVL(V_TIME, 0), V_START, V_END);

        V_IDX := V_IDX + 1;
    END LOOP;

    O_TONGDIEM := V_TONG;
    O_DAT := CASE WHEN V_TONG >= 35 THEN 1 ELSE 0 END;
END;
/
CREATE OR REPLACE PROCEDURE PROC_MP_ONTAP_INDEX
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(C.CHUONGMOPHONGID AS NUMBER(10)) AS IDCHUONGMP,
            CAST(NVL(C.TENCHUONG, '') AS NVARCHAR2(255)) AS TENCHUONG,
            CAST(NVL(C.THUTU, 0) AS NUMBER(10)) AS CHUONG_THUTU,

            CAST(TH.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(TH.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(TH.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,
            CAST(NVL(TH.THUTU, 0) AS NUMBER(10)) AS TINHHUONG_THUTU,
            CAST(NVL(TH.KHO, 0) AS NUMBER(1)) AS KHO,

            CAST(NVL(TH.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(TH.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC
        FROM CHUONGMOPHONG C
        LEFT JOIN TINHHUONGMOPHONG TH
            ON TH.CHUONGMOPHONGID = C.CHUONGMOPHONGID
        ORDER BY C.THUTU, TH.THUTU, TH.TINHHUONGMOPHONGID;
END;
/
-- KHÓA HỌC V2
CREATE OR REPLACE PROCEDURE SP_KHOAHOC_CHECK_DANGKY
(
    p_userId    IN NUMBER,
    p_khoaHocId IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        WITH KHOA_HOC_TARGET AS
        (
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.diaDiem,
                kh.trangThai,
                hg.hangId,
                hg.tenHang,
                hg.moTa,
                hg.loaiPhuongTien,
                hg.soCauHoi,
                hg.diemDat,
                hg.thoiGianTn,
                hg.hocPhi
            FROM KhoaHoc kh
            JOIN HangGplx hg ON hg.hangId = kh.hangId
            WHERE kh.khoaHocId = p_khoaHocId
        ),
        HOC_VIEN_USER AS
        (
            SELECT hv.hocVienId
            FROM HocVien hv
            WHERE hv.userId = p_userId
        ),
        HO_SO_CUNG_HANG AS
        (
            SELECT
                hs.hoSoId,
                hs.tenHoSo,
                hs.ngayDangKy,
                hs.trangThai AS trangThaiHoSo,
                hs.hangId,
                ROW_NUMBER() OVER
                (
                    ORDER BY NVL(hs.ngayDangKy, DATE '1900-01-01') DESC, hs.hoSoId DESC
                ) AS rn
            FROM HoSoThiSinh hs
            JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = hs.hocVienId
            JOIN KHOA_HOC_TARGET kht ON kht.hangId = hs.hangId
        ),
        HO_SO_PHU_HOP_DA_DUYET AS
        (
            SELECT *
            FROM
            (
                SELECT
                    hs.hoSoId,
                    hs.tenHoSo,
                    hs.ngayDangKy,
                    hs.trangThaiHoSo,
                    hs.hangId,
                    ROW_NUMBER() OVER
                    (
                        ORDER BY NVL(hs.ngayDangKy, DATE '1900-01-01') DESC, hs.hoSoId DESC
                    ) AS rn2
                FROM HO_SO_CUNG_HANG hs
                WHERE hs.trangThaiHoSo = N'Đã duyệt'
            )
            WHERE rn2 = 1
        ),
        HO_SO_CHUA_DUYET AS
        (
            SELECT COUNT(*) AS soHoSoChuaDuyet
            FROM HO_SO_CUNG_HANG
            WHERE trangThaiHoSo <> N'Đã duyệt' OR trangThaiHoSo IS NULL
        ),
        DA_HOC_HANG AS
        (
            SELECT COUNT(*) AS soLanDaHoc
            FROM KetQuaHocTap kq
            JOIN HoSoThiSinh hs ON hs.hoSoId = kq.hoSoId
            JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = hs.hocVienId
            JOIN ChiTietKetQuaHocTap ct ON ct.ketQuaHocTapId = kq.ketQuaHocTapId
            JOIN KhoaHoc kh ON kh.khoaHocId = ct.khoaHocId
            JOIN KHOA_HOC_TARGET kht ON kht.hangId = kh.hangId
        ),
        KHOA_HOC_DA_DANG_KY AS
        (
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.trangThai,
                hg.hangId,
                hg.tenHang,
                ct.phieuId,
                pt.ngayLap,
                pt.ngayNop,
                ROW_NUMBER() OVER
                (
                    PARTITION BY ct.phieuId
                    ORDER BY pt.phieuId DESC
                ) AS rn
            FROM HocVien hv
            JOIN HoSoThiSinh hs
                ON hs.hocVienId = hv.hocVienId
            JOIN ChiTietPhieuThanhToan ct
                ON ct.hoSoId = hs.hoSoId
            JOIN PhieuThanhToan pt
                ON pt.phieuId = ct.phieuId
            JOIN KetQuaHocTap kq
                ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
            JOIN ChiTietKetQuaHocTap ct_kq
                ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
            JOIN KhoaHoc kh
                ON kh.khoaHocId = ct_kq.khoaHocId
            JOIN HangGplx hg
                ON hg.hangId = kh.hangId
            WHERE hv.userId = p_userId
        ),
        KHOA_HOC_DA_DANG_KY_DISTINCT AS
        (
            SELECT *
            FROM KHOA_HOC_DA_DANG_KY
            WHERE rn = 1
        ),
        DA_DANG_KY_CHINH_KHOA_HOC AS
        (
            SELECT
                COUNT(*) AS soLuongDangKy,
                MAX(khdk.khoaHocId) AS khoaHocIdDaDangKy,
                MAX(khdk.tenKhoaHoc) KEEP
                (
                    DENSE_RANK LAST ORDER BY khdk.phieuId
                ) AS tenKhoaHocDaDangKy
            FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
            JOIN KHOA_HOC_TARGET kht
                ON kht.khoaHocId = khdk.khoaHocId
        ),
        DANG_TRUNG_THOI_GIAN AS
        (
            SELECT
                COUNT(*) AS soLuongTrung,
                MAX(khdk.khoaHocId) AS khoaHocIdTrung,
                MAX(khdk.tenKhoaHoc) KEEP
                (
                    DENSE_RANK LAST ORDER BY khdk.phieuId
                ) AS tenKhoaHocTrung,
                MAX(khdk.ngayBatDau) KEEP
                (
                    DENSE_RANK LAST ORDER BY khdk.phieuId
                ) AS ngayBatDauTrung,
                MAX(khdk.ngayKetThuc) KEEP
                (
                    DENSE_RANK LAST ORDER BY khdk.phieuId
                ) AS ngayKetThucTrung
            FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
            CROSS JOIN KHOA_HOC_TARGET kht
            WHERE khdk.ngayBatDau IS NOT NULL
              AND khdk.ngayKetThuc IS NOT NULL
              AND kht.ngayBatDau IS NOT NULL
              AND kht.ngayKetThuc IS NOT NULL
              AND khdk.ngayBatDau <= kht.ngayKetThuc
              AND khdk.ngayKetThuc >= kht.ngayBatDau
        ),
        DA_DANG_KY_CUNG_HANG AS
        (
            SELECT
                COUNT(*) AS soLuongCungHang,
                MAX(khdk.khoaHocId) AS khoaHocIdCungHang,
                MAX(khdk.tenKhoaHoc) KEEP
                (
                    DENSE_RANK LAST ORDER BY khdk.phieuId
                ) AS tenKhoaHocCungHang
            FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
            JOIN KHOA_HOC_TARGET kht
                ON kht.hangId = khdk.hangId
            WHERE khdk.khoaHocId <> kht.khoaHocId
        )
        SELECT
            kht.khoaHocId,
            kht.tenKhoaHoc,
            kht.ngayBatDau,
            kht.ngayKetThuc,
            kht.diaDiem,
            kht.trangThai,
            kht.hangId,
            kht.tenHang,
            kht.moTa,
            kht.loaiPhuongTien,
            kht.soCauHoi,
            kht.diemDat,
            kht.thoiGianTn,
            kht.hocPhi,

            CASE
                WHEN kht.trangThai = N'Sắp khai giảng' THEN 1
                ELSE 0
            END AS isMoDangKy,

            CASE
                WHEN hsp.hoSoId IS NOT NULL THEN 1
                ELSE 0
            END AS hasHoSoPhuHop,

            CASE
                WHEN NVL(hscd.soHoSoChuaDuyet, 0) > 0 AND hsp.hoSoId IS NULL THEN 1
                ELSE 0
            END AS hasHoSoChuaDuyet,

            NVL(hsp.hoSoId, 0) AS hoSoIdPhuHop,
            NVL(hsp.tenHoSo, N'') AS tenHoSoPhuHop,
            hsp.ngayDangKy AS ngayDangKyHoSo,
            NVL(hsp.trangThaiHoSo, N'') AS trangThaiHoSo,

            CASE
                WHEN NVL(dhh.soLanDaHoc, 0) > 0 THEN 1
                ELSE 0
            END AS daTungHocHang,

            NVL(dhh.soLanDaHoc, 0) AS soLanDaHocHang,

            CASE
                WHEN NVL(ddkckh.soLuongDangKy, 0) > 0 THEN 1
                ELSE 0
            END AS daDangKyChinhKhoaHoc,

            NVL(ddkckh.khoaHocIdDaDangKy, 0) AS khoaHocIdDaDangKy,
            NVL(ddkckh.tenKhoaHocDaDangKy, N'') AS tenKhoaHocDaDangKy,

            CASE
                WHEN NVL(dttg.soLuongTrung, 0) > 0 THEN 1
                ELSE 0
            END AS biTrungThoiGianHoc,

            NVL(dttg.khoaHocIdTrung, 0) AS khoaHocIdTrungThoiGian,
            NVL(dttg.tenKhoaHocTrung, N'') AS tenKhoaHocTrungThoiGian,
            dttg.ngayBatDauTrung AS ngayBatDauTrungThoiGian,
            dttg.ngayKetThucTrung AS ngayKetThucTrungThoiGian,

            CASE
                WHEN NVL(ddkch.soLuongCungHang, 0) > 0 THEN 1
                ELSE 0
            END AS daTungDangKyCungHang,

            NVL(ddkch.khoaHocIdCungHang, 0) AS khoaHocIdCungHangGanNhat,
            NVL(ddkch.tenKhoaHocCungHang, N'') AS tenKhoaHocCungHangGanNhat,

            CASE
                WHEN NVL(ddkckh.soLuongDangKy, 0) > 0 THEN 0
                WHEN NVL(dttg.soLuongTrung, 0) > 0 THEN 0
                WHEN kht.trangThai <> N'Sắp khai giảng' THEN 0
                WHEN hsp.hoSoId IS NULL THEN 0
                ELSE 1
            END AS coTheDangKy
        FROM KHOA_HOC_TARGET kht
        LEFT JOIN HO_SO_PHU_HOP_DA_DUYET hsp ON 1 = 1
        LEFT JOIN HO_SO_CHUA_DUYET hscd ON 1 = 1
        LEFT JOIN DA_HOC_HANG dhh ON 1 = 1
        LEFT JOIN DA_DANG_KY_CHINH_KHOA_HOC ddkckh ON 1 = 1
        LEFT JOIN DANG_TRUNG_THOI_GIAN dttg ON 1 = 1
        LEFT JOIN DA_DANG_KY_CUNG_HANG ddkch ON 1 = 1;
END;
/

-- THHANH TOÁN
CREATE OR REPLACE PROCEDURE SP_PAYMENT_START
(
    p_userId    IN NUMBER,
    p_hoSoId    IN NUMBER,
    p_khoaHocId IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
    v_hocVienId           NUMBER;
    v_hoTenHocVien        NVARCHAR2(100);
    v_hangHoSoId          NUMBER;
    v_hangKhoaHocId       NUMBER;
    v_tenKhoaHoc          NVARCHAR2(200);
    v_tenHang             NVARCHAR2(100);
    v_hocPhi              NUMBER(18,2);
    v_tenPhieu            NVARCHAR2(300);
    v_noiDungMacDinh      NVARCHAR2(500);
    v_phieuId             NUMBER;
    v_ketQuaHocTapId      NUMBER;
BEGIN
    SELECT hv.hocVienId, hv.hoTen
    INTO v_hocVienId, v_hoTenHocVien
    FROM HocVien hv
    JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
    WHERE hv.userId = p_userId
      AND hs.hoSoId = p_hoSoId;

    SELECT hs.hangId
    INTO v_hangHoSoId
    FROM HoSoThiSinh hs
    WHERE hs.hoSoId = p_hoSoId
      AND hs.trangThai = N'Đã duyệt';

    SELECT kh.hangId,
           kh.tenKhoaHoc,
           hg.tenHang,
           hg.hocPhi
    INTO v_hangKhoaHocId,
         v_tenKhoaHoc,
         v_tenHang,
         v_hocPhi
    FROM KhoaHoc kh
    JOIN HangGplx hg ON hg.hangId = kh.hangId
    WHERE kh.khoaHocId = p_khoaHocId
      AND kh.trangThai = N'Sắp khai giảng';

    IF v_hangHoSoId <> v_hangKhoaHocId THEN
        OPEN p_cursor FOR
            SELECT
                0 AS isValid,
                N'Hồ sơ không phù hợp với khóa học.' AS message,
                CAST(NULL AS NUMBER) AS phieuId,
                CAST(NULL AS NUMBER) AS khoaHocId,
                CAST(NULL AS NVARCHAR2(200)) AS tenKhoaHoc,
                CAST(NULL AS NUMBER) AS hoSoId,
                CAST(NULL AS NVARCHAR2(100)) AS hoTenHocVien,
                CAST(NULL AS NVARCHAR2(100)) AS tenHang,
                CAST(NULL AS NUMBER(18,2)) AS soTien,
                CAST(NULL AS DATE) AS ngayLap,
                CAST(NULL AS NVARCHAR2(500)) AS noiDungMacDinh,
                CAST(NULL AS NVARCHAR2(100)) AS phuongThuc
            FROM dual;
        RETURN;
    END IF;

    v_tenPhieu := N'Thanh toán khóa học ' || v_tenKhoaHoc || N' - ' || v_hoTenHocVien;
    v_noiDungMacDinh := v_hoTenHocVien || N' Thanh toán khóa học ' || v_tenKhoaHoc;

    INSERT INTO PhieuThanhToan
    (
        tenPhieu,
        ngayLap,
        tongTien,
        ngayNop,
        phuongThuc
    )
    VALUES
    (
        v_tenPhieu,
        SYSTIMESTAMP,
        v_hocPhi,
        NULL,
        NULL
    )
    RETURNING phieuId INTO v_phieuId;

    INSERT INTO KetQuaHocTap
    (
        hoSoId,
        nhanXet,
        soBuoiHoc,
        soBuoiVang,
        soKmHoanThanh,
        DU_DK_THITOTNGHIEP,
        DAUTOTNGHIEP,
        DU_DK_THISATHACH,
        THOIGIANCAPNHAT
    )
    VALUES
    (
        p_hoSoId,
        NULL,
        NULL,
        NULL,
        NULL,
        0,
        0,
        0,
        SYSDATE
    )
    RETURNING ketQuaHocTapId INTO v_ketQuaHocTapId;

    INSERT INTO ChiTietKetQuaHocTap
    (
        ketQuaHocTapId,
        khoaHocId,
        lyThuyetKq,
        saHinhKq,
        duongTruongKq,
        moPhongKq
    )
    VALUES
    (
        v_ketQuaHocTapId,
        p_khoaHocId,
        NULL,
        NULL,
        NULL,
        NULL
    );

    INSERT INTO ChiTietPhieuThanhToan
    (
        hoSoId,
        phieuId,
        loaiPhi,
        ghiChu,
        ketQuaHocTapId
    )
    VALUES
    (
        p_hoSoId,
        v_phieuId,
        N'Khóa học',
        NULL,
        v_ketQuaHocTapId
    );

    OPEN p_cursor FOR
        SELECT
            1 AS isValid,
            N'Hợp lệ' AS message,
            pt.phieuId,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hs.hoSoId,
            hv.hoTen AS hoTenHocVien,
            hg.tenHang,
            pt.tongTien AS soTien,
            pt.ngayLap,
            v_noiDungMacDinh AS noiDungMacDinh,
            NVL(pt.phuongThuc, N'') AS phuongThuc
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
        JOIN KetQuaHocTap kq ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
        JOIN ChiTietKetQuaHocTap ct_kq ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
        JOIN KhoaHoc kh ON kh.khoaHocId = ct_kq.khoaHocId
        JOIN HangGplx hg ON hg.hangId = kh.hangId
        WHERE pt.phieuId = v_phieuId
          AND hs.hoSoId = p_hoSoId;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        OPEN p_cursor FOR
            SELECT
                0 AS isValid,
                N'Không đủ điều kiện tạo phiếu thanh toán.' AS message,
                CAST(NULL AS NUMBER) AS phieuId,
                CAST(NULL AS NUMBER) AS khoaHocId,
                CAST(NULL AS NVARCHAR2(200)) AS tenKhoaHoc,
                CAST(NULL AS NUMBER) AS hoSoId,
                CAST(NULL AS NVARCHAR2(100)) AS hoTenHocVien,
                CAST(NULL AS NVARCHAR2(100)) AS tenHang,
                CAST(NULL AS NUMBER(18,2)) AS soTien,
                CAST(NULL AS DATE) AS ngayLap,
                CAST(NULL AS NVARCHAR2(500)) AS noiDungMacDinh,
                CAST(NULL AS NVARCHAR2(100)) AS phuongThuc
            FROM dual;
END;
/

CREATE OR REPLACE PROCEDURE SP_PAYMENT_CHOOSE_METHOD
(
    p_userId      IN NUMBER,
    p_phieuId     IN NUMBER,
    p_method      IN NVARCHAR2,
    p_noiDung     IN NVARCHAR2,
    o_result      OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    -- kiểm tra phiếu có thuộc user không
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan pt
    JOIN ChiTietPhieuThanhToan ct ON ct.phieuId = pt.phieuId
    JOIN HoSoThiSinh hs ON hs.hoSoId = ct.hoSoId
    JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
    WHERE pt.phieuId = p_phieuId
      AND hv.userId = p_userId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET phuongThuc = p_method
    WHERE phieuId = p_phieuId;

    UPDATE ChiTietPhieuThanhToan
    SET ghiChu = p_noiDung
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_GET_VNPAY_INFO
(
    p_userId    IN NUMBER,
    p_phieuId   IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.tongTien,
            pt.ngayNop,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.hoSoId,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KhoaHoc kh
            ON kh.hangId = hs.hangId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE pt.phieuId = p_phieuId
          AND hv.userId = p_userId
          AND ct.loaiPhi = N'Khóa học'
        ORDER BY kh.khoaHocId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_VNPAY_FAIL
(
    p_phieuId IN NUMBER,
    o_result  OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan
    WHERE phieuId = p_phieuId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET ngayNop = NULL
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_VNPAY_SUCCESS
(
    p_phieuId IN NUMBER,
    o_result  OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan
    WHERE phieuId = p_phieuId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET ngayNop = SYSTIMESTAMP,
        phuongThuc = N'VNPAY'
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
--thanh toán PAYPAL
CREATE OR REPLACE PROCEDURE SP_PAYMENT_GET_PAYPAL_INFO
(
    p_userId    IN NUMBER,
    p_phieuId   IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.tongTien,
            pt.ngayNop,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.hoSoId,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KhoaHoc kh
            ON kh.hangId = hs.hangId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE pt.phieuId = p_phieuId
          AND hv.userId = p_userId
          AND ct.loaiPhi = N'Khóa học'
        ORDER BY kh.khoaHocId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_PAYPAL_SUCCESS
(
    p_phieuId IN NUMBER,
    o_result  OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan
    WHERE phieuId = p_phieuId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET ngayNop = SYSTIMESTAMP,
        phuongThuc = N'PAYPAL'
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_PAYPAL_FAIL
(
    p_phieuId IN NUMBER,
    o_result  OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan
    WHERE phieuId = p_phieuId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET ngayNop = NULL
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_GET_PAYPAL_INFO_BY_PHIEU
(
    p_phieuId   IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.tongTien,
            pt.ngayNop,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.hoSoId,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KhoaHoc kh
            ON kh.hangId = hs.hangId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE pt.phieuId = p_phieuId
          AND ct.loaiPhi = N'Khóa học'
        ORDER BY kh.khoaHocId DESC;
END;
/
-- Thanh toán MOMO
CREATE OR REPLACE PROCEDURE SP_PAYMENT_GET_MOMO_INFO
(
    p_userId    IN NUMBER,
    p_phieuId   IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.tongTien,
            pt.ngayNop,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.hoSoId,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KhoaHoc kh
            ON kh.hangId = hs.hangId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE pt.phieuId = p_phieuId
          AND hv.userId = p_userId
          AND ct.loaiPhi = N'Khóa học'
        ORDER BY kh.khoaHocId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_GET_MOMO_INFO_BY_PHIEU
(
    p_phieuId   IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.tongTien,
            pt.ngayNop,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.hoSoId,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KhoaHoc kh
            ON kh.hangId = hs.hangId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE pt.phieuId = p_phieuId
          AND ct.loaiPhi = N'Khóa học'
        ORDER BY kh.khoaHocId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_PAYMENT_MOMO_SUCCESS
(
    p_phieuId IN NUMBER,
    o_result  OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM PhieuThanhToan
    WHERE phieuId = p_phieuId;

    IF v_count = 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    UPDATE PhieuThanhToan
    SET ngayNop = SYSTIMESTAMP,
        phuongThuc = N'MOMO'
    WHERE phieuId = p_phieuId;

    o_result := 1;
END;
/
-- Lịch sử thanh toán
CREATE OR REPLACE PROCEDURE SP_PAYMENT_HISTORY_BY_USER
(
    p_userId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.ngayNop,
            pt.tongTien,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.loaiPhi,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.hoSoId,
            hs.tenHoSo,
            hv.hoTen AS hoTenHocVien,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang,
            CASE
                WHEN pt.ngayNop IS NOT NULL THEN N'Đã thanh toán'
                ELSE N'Chưa thanh toán'
            END AS trangThaiThanhToan,
            CASE
                WHEN pt.ngayNop IS NOT NULL THEN 1
                ELSE 0
            END AS coTheTaiHoaDon
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KetQuaHocTap kq
            ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
        JOIN ChiTietKetQuaHocTap ct_kq
            ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
        JOIN KhoaHoc kh
            ON kh.khoaHocId = ct_kq.khoaHocId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE hv.userId = p_userId
        ORDER BY pt.phieuId DESC;
END;
/

CREATE OR REPLACE PROCEDURE SP_PAYMENT_HISTORY_DETAIL
(
    p_userId  IN NUMBER,
    p_phieuId IN NUMBER,
    p_cursor  OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.ngayNop,
            pt.tongTien,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.loaiPhi,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.hoSoId,
            hs.tenHoSo,
            hs.trangThai AS trangThaiHoSo,
            hv.hocVienId,
            hv.hoTen AS hoTenHocVien,
            hv.sdt,
            hv.email,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.diaDiem,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.trangThai AS trangThaiKhoaHoc,
            hg.hangId,
            hg.tenHang,
            hg.loaiPhuongTien,
            hg.hocPhi,
            CASE
                WHEN pt.ngayNop IS NOT NULL THEN N'Đã thanh toán'
                ELSE N'Chưa thanh toán'
            END AS trangThaiThanhToan
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KetQuaHocTap kq
            ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
        JOIN ChiTietKetQuaHocTap ct_kq
            ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
        JOIN KhoaHoc kh
            ON kh.khoaHocId = ct_kq.khoaHocId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE hv.userId = p_userId
          AND pt.phieuId = p_phieuId;
END;
/

CREATE OR REPLACE PROCEDURE SP_PAYMENT_INVOICE_DETAIL
(
    p_userId  IN NUMBER,
    p_phieuId IN NUMBER,
    p_cursor  OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.ngayNop,
            pt.tongTien,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            ct.loaiPhi,
            NVL(ct.ghiChu, N'') AS ghiChu,
            hs.hoSoId,
            hs.tenHoSo,
            hs.trangThai AS trangThaiHoSo,
            hv.hocVienId,
            hv.hoTen AS hoTenHocVien,
            hv.sdt,
            hv.email,
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.diaDiem,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.trangThai AS trangThaiKhoaHoc,
            hg.hangId,
            hg.tenHang,
            hg.loaiPhuongTien,
            hg.hocPhi,
            CASE
                WHEN pt.ngayNop IS NOT NULL THEN N'Đã thanh toán'
                ELSE N'Chưa thanh toán'
            END AS trangThaiThanhToan
        FROM PhieuThanhToan pt
        JOIN ChiTietPhieuThanhToan ct
            ON ct.phieuId = pt.phieuId
        JOIN HoSoThiSinh hs
            ON hs.hoSoId = ct.hoSoId
        JOIN HocVien hv
            ON hv.hocVienId = hs.hocVienId
        JOIN KetQuaHocTap kq
            ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
        JOIN ChiTietKetQuaHocTap ct_kq
            ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
        JOIN KhoaHoc kh
            ON kh.khoaHocId = ct_kq.khoaHocId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE hv.userId = p_userId
          AND pt.phieuId = p_phieuId
          AND pt.ngayNop IS NOT NULL;
END;
/

CREATE OR REPLACE PROCEDURE PROC_GET_USER_DASHBOARD
(
    p_userId IN NUMBER,
    o_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN o_cursor FOR
        SELECT
            u.userId,
            u.userName,
            u.isActive,

            hv.hocVienId,
            hv.hoTen,
            hv.email,
            hv.soCmndCccd,
            hv.namSinh,
            hv.gioiTinh,
            hv.sdt,
            hv.avatarUrl,

            hs.hoSoId,
            hs.trangThai AS hoSoTrangThai,
            hs.ngayDangKy,

            hg.tenHang,

            /* tổng buổi học */
            NVL(kq.soBuoiHoc, 0) AS soBuoiHoc,

            /* số kỳ thi đã đăng ký */
            (
                SELECT COUNT(*)
                FROM ChiTietDangKyThi ct
                WHERE ct.hoSoId = hs.hoSoId
            ) AS soKyThi,

            /* tổng thanh toán */
            (
                SELECT NVL(SUM(p.tongTien),0)
                FROM ChiTietPhieuThanhToan ctpt
                JOIN PhieuThanhToan p ON p.phieuId = ctpt.phieuId
                WHERE ctpt.hoSoId = hs.hoSoId
            ) AS tongThanhToan,

            /* GPLX */
            gplx.trangThai AS gplxTrangThai

        FROM "User" u
        LEFT JOIN HocVien hv ON hv.userId = u.userId
        LEFT JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
        LEFT JOIN HangGplx hg ON hg.hangId = hs.hangId
        LEFT JOIN KetQuaHocTap kq ON kq.hoSoId = hs.hoSoId
        LEFT JOIN GiayPhepLaiXe gplx ON gplx.hoSoId = hs.hoSoId

        WHERE u.userId = p_userId;
END;
/
CREATE OR REPLACE PROCEDURE SP_MY_COURSES_BY_USER
(
    p_userId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai AS trangThaiKhoaHocGoc,
            hg.hangId,
            hg.tenHang,
            hg.loaiPhuongTien,
            hg.hocPhi,
            hs.hoSoId,
            hs.tenHoSo,
            hv.hocVienId,
            hv.hoTen AS hoTenHocVien,
            pt.phieuId,
            pt.tenPhieu,
            pt.ngayLap,
            pt.ngayNop,
            pt.tongTien,
            NVL(pt.phuongThuc, N'') AS phuongThuc,
            NVL(ct.loaiPhi, N'') AS loaiPhi,
            NVL(ct.ghiChu, N'') AS ghiChu,
            kq.ketQuaHocTapId,
            ct_kq.lyThuyetKq,
            ct_kq.saHinhKq,
            ct_kq.duongTruongKq,
            ct_kq.moPhongKq,
            CASE
                WHEN NVL(ct_kq.lyThuyetKq, 0) = 1
                 AND NVL(ct_kq.saHinhKq, 0) = 1
                 AND NVL(ct_kq.duongTruongKq, 0) = 1
                 AND NVL(ct_kq.moPhongKq, 0) = 1
                THEN N'Hoàn thành'
                WHEN TRUNC(SYSDATE) BETWEEN TRUNC(kh.ngayBatDau) AND TRUNC(kh.ngayKetThuc)
                THEN N'Đang học'
                WHEN TRUNC(SYSDATE) > TRUNC(kh.ngayKetThuc)
                     AND NOT (
                        NVL(ct_kq.lyThuyetKq, 0) = 1
                        AND NVL(ct_kq.saHinhKq, 0) = 1
                        AND NVL(ct_kq.duongTruongKq, 0) = 1
                        AND NVL(ct_kq.moPhongKq, 0) = 1
                     )
                THEN N'Không hoàn thành'
                ELSE N'Chưa bắt đầu'
            END AS trangThaiHocTap,
            CASE
                WHEN NVL(ct_kq.lyThuyetKq, 0) = 1
                 AND NVL(ct_kq.saHinhKq, 0) = 1
                 AND NVL(ct_kq.duongTruongKq, 0) = 1
                 AND NVL(ct_kq.moPhongKq, 0) = 1
                THEN 1
                ELSE 0
            END AS daHoanThanh,
            CASE
                WHEN TRUNC(SYSDATE) BETWEEN TRUNC(kh.ngayBatDau) AND TRUNC(kh.ngayKetThuc)
                     AND NOT (
                        NVL(ct_kq.lyThuyetKq, 0) = 1
                        AND NVL(ct_kq.saHinhKq, 0) = 1
                        AND NVL(ct_kq.duongTruongKq, 0) = 1
                        AND NVL(ct_kq.moPhongKq, 0) = 1
                     )
                THEN 1
                ELSE 0
            END AS dangHoc,
            CASE
                WHEN TRUNC(SYSDATE) > TRUNC(kh.ngayKetThuc)
                     AND NOT (
                        NVL(ct_kq.lyThuyetKq, 0) = 1
                        AND NVL(ct_kq.saHinhKq, 0) = 1
                        AND NVL(ct_kq.duongTruongKq, 0) = 1
                        AND NVL(ct_kq.moPhongKq, 0) = 1
                     )
                THEN 1
                ELSE 0
            END AS khongHoanThanh
        FROM HocVien hv
        JOIN HoSoThiSinh hs
            ON hs.hocVienId = hv.hocVienId
        JOIN ChiTietPhieuThanhToan ct
            ON ct.hoSoId = hs.hoSoId
        JOIN PhieuThanhToan pt
            ON pt.phieuId = ct.phieuId
        JOIN KetQuaHocTap kq
            ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
        JOIN ChiTietKetQuaHocTap ct_kq
            ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
        JOIN KhoaHoc kh
            ON kh.khoaHocId = ct_kq.khoaHocId
        JOIN HangGplx hg
            ON hg.hangId = kh.hangId
        WHERE hv.userId = p_userId
          AND pt.ngayNop IS NOT NULL
        ORDER BY kh.ngayBatDau DESC, pt.phieuId DESC;
END;
/

CREATE OR REPLACE PROCEDURE SP_MY_COURSE_DETAIL
(
    p_userId    IN NUMBER,
    p_khoaHocId IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT *
        FROM
        (
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.diaDiem,
                kh.trangThai AS trangThaiKhoaHocGoc,
                hg.hangId,
                hg.tenHang,
                hg.moTa,
                hg.loaiPhuongTien,
                hg.soCauHoi,
                hg.diemDat,
                hg.thoiGianTn,
                hg.hocPhi,

                hv.hocVienId,
                hv.hoTen AS hoTenHocVien,
                hv.sdt,
                hv.email,

                u.userId,
                u.userName,
                u.isActive,

                hs.hoSoId,
                hs.tenHoSo,
                hs.ngayDangKy,
                hs.trangThai AS trangThaiHoSo,
                NVL(hs.ghiChu, N'') AS ghiChuHoSo,

                pt.phieuId,
                pt.tenPhieu,
                pt.ngayLap,
                pt.ngayNop,
                pt.tongTien,
                NVL(pt.phuongThuc, N'') AS phuongThuc,
                NVL(ct.loaiPhi, N'') AS loaiPhi,
                NVL(ct.ghiChu, N'') AS ghiChuThanhToan,

                kq.ketQuaHocTapId,
                NVL(kq.nhanXet, N'') AS nhanXet,
                kq.soBuoiHoc,
                kq.soBuoiVang,
                NVL(kq.soKmHoanThanh, N'') AS soKmHoanThanh,
                NVL(kq.DU_DK_THITOTNGHIEP, 0) AS DU_DK_THITOTNGHIEP,
                NVL(kq.DAUTOTNGHIEP, 0) AS DAUTOTNGHIEP,
                NVL(kq.DU_DK_THISATHACH, 0) AS DU_DK_THISATHACH,
                kq.THOIGIANCAPNHAT,

                ct_kq.lyThuyetKq,
                ct_kq.saHinhKq,
                ct_kq.duongTruongKq,
                ct_kq.moPhongKq,

                CASE
                    WHEN NVL(ct_kq.lyThuyetKq, 0) = 1
                     AND NVL(ct_kq.saHinhKq, 0) = 1
                     AND NVL(ct_kq.duongTruongKq, 0) = 1
                     AND NVL(ct_kq.moPhongKq, 0) = 1
                    THEN N'Hoàn thành'
                    WHEN TRUNC(SYSDATE) BETWEEN TRUNC(kh.ngayBatDau) AND TRUNC(kh.ngayKetThuc)
                    THEN N'Đang học'
                    WHEN TRUNC(SYSDATE) > TRUNC(kh.ngayKetThuc)
                         AND NOT (
                            NVL(ct_kq.lyThuyetKq, 0) = 1
                            AND NVL(ct_kq.saHinhKq, 0) = 1
                            AND NVL(ct_kq.duongTruongKq, 0) = 1
                            AND NVL(ct_kq.moPhongKq, 0) = 1
                         )
                    THEN N'Không hoàn thành'
                    ELSE N'Chưa bắt đầu'
                END AS trangThaiHocTap,

                ROW_NUMBER() OVER
                (
                    ORDER BY pt.ngayNop DESC, pt.phieuId DESC, kq.ketQuaHocTapId DESC
                ) AS rn
            FROM HocVien hv
            JOIN "User" u
                ON u.userId = hv.userId
            JOIN HoSoThiSinh hs
                ON hs.hocVienId = hv.hocVienId
            JOIN ChiTietPhieuThanhToan ct
                ON ct.hoSoId = hs.hoSoId
            JOIN PhieuThanhToan pt
                ON pt.phieuId = ct.phieuId
            JOIN KetQuaHocTap kq
                ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
            JOIN ChiTietKetQuaHocTap ct_kq
                ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
            JOIN KhoaHoc kh
                ON kh.khoaHocId = ct_kq.khoaHocId
            JOIN HangGplx hg
                ON hg.hangId = kh.hangId
            WHERE hv.userId = p_userId
              AND kh.khoaHocId = p_khoaHocId
              AND pt.ngayNop IS NOT NULL
        )
        WHERE rn = 1;
END;
/
-- HỒ SƠ THÍ SINH
CREATE OR REPLACE VIEW VW_DANH_SACH_HOSO AS
SELECT 
    hs.hoSoId,
    hv.hoTen,
    hv.sdt,
    hg.tenHang,
    hs.ngayDangKy,
    hs.trangThai,
    pksk.thoiHan,
    
    FLOOR(MONTHS_BETWEEN(ADD_MONTHS(hs.ngayDangKy, 12), SYSDATE)) AS soThangConLai,
    FLOOR(ADD_MONTHS(hs.ngayDangKy, 12) - SYSDATE) AS soNgayConLai

FROM HoSoThiSinh hs
JOIN HocVien hv ON hs.hocVienId = hv.hocVienId
JOIN HangGplx hg ON hs.hangId = hg.hangId
LEFT JOIN PhieuKhamSucKhoe pksk ON hs.khamSucKhoeId = pksk.khamSucKhoeId;
/

CREATE OR REPLACE PROCEDURE SP_GET_MY_HOSO (
    p_userId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT 
        hs.hoSoId,
        hv.hoTen,
        hv.avatarUrl,
        hs.tenHoSo,
        hg.tenHang,
        hs.ngayDangKy,
        hs.trangThai,
        FLOOR(MONTHS_BETWEEN(ADD_MONTHS(hs.ngayDangKy, 12), SYSDATE)) AS soThangConLai,
        FLOOR(ADD_MONTHS(hs.ngayDangKy, 12) - SYSDATE) AS soNgayConLai
    FROM HoSoThiSinh hs
    JOIN HocVien hv ON hs.hocVienId = hv.hocVienId
    JOIN HangGplx hg ON hs.hangId = hg.hangId
    WHERE hv.userId = p_userId
    ORDER BY hs.ngayDangKy DESC;
END;
/

CREATE OR REPLACE PROCEDURE SP_GET_MY_HOSO_DETAIL (
    p_hoSoId IN NUMBER,
    p_userId IN NUMBER,
    p_info OUT SYS_REFCURSOR,
    p_images OUT SYS_REFCURSOR
)
AS
    v_khamSucKhoeId NUMBER;
BEGIN
    SELECT hs.khamSucKhoeId
    INTO v_khamSucKhoeId
    FROM HoSoThiSinh hs
    JOIN HocVien hv ON hs.hocVienId = hv.hocVienId
    WHERE hs.hoSoId = p_hoSoId
      AND hv.userId = p_userId;

    OPEN p_info FOR
    SELECT 
        hs.hoSoId,
        hv.hoTen,
        hv.soCmndCccd,
        hv.namSinh,
        hv.gioiTinh,
        hv.sdt,
        hv.email,
        hv.avatarUrl,
        hs.tenHoSo,
        hs.loaiHoSo,
        hs.ngayDangKy,
        hs.trangThai,
        hs.ghiChu,
        hg.tenHang,
        pksk.hieuLuc,
        pksk.thoiHan,
        pksk.khamMat,
        pksk.huyetAp,
        pksk.chieuCao,
        pksk.canNang
    FROM HoSoThiSinh hs
    JOIN HocVien hv ON hs.hocVienId = hv.hocVienId
    JOIN HangGplx hg ON hs.hangId = hg.hangId
    LEFT JOIN PhieuKhamSucKhoe pksk ON hs.khamSucKhoeId = pksk.khamSucKhoeId
    WHERE hs.hoSoId = p_hoSoId
      AND hv.userId = p_userId;

    OPEN p_images FOR
    SELECT ag.urlAnh
    FROM AnhGksk ag
    WHERE ag.khamSucKhoeId = v_khamSucKhoeId
    ORDER BY ag.anhId;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        OPEN p_info FOR
        SELECT 
            CAST(NULL AS NUMBER) AS hoSoId,
            CAST(NULL AS NVARCHAR2(100)) AS hoTen,
            CAST(NULL AS NVARCHAR2(20)) AS soCmndCccd,
            CAST(NULL AS DATE) AS namSinh,
            CAST(NULL AS NVARCHAR2(10)) AS gioiTinh,
            CAST(NULL AS NVARCHAR2(15)) AS sdt,
            CAST(NULL AS NVARCHAR2(50)) AS email,
            CAST(NULL AS NVARCHAR2(500)) AS avatarUrl,
            CAST(NULL AS NVARCHAR2(100)) AS tenHoSo,
            CAST(NULL AS NVARCHAR2(50)) AS loaiHoSo,
            CAST(NULL AS DATE) AS ngayDangKy,
            CAST(NULL AS NVARCHAR2(50)) AS trangThai,
            CAST(NULL AS NVARCHAR2(255)) AS ghiChu,
            CAST(NULL AS NVARCHAR2(20)) AS tenHang,
            CAST(NULL AS NVARCHAR2(50)) AS hieuLuc,
            CAST(NULL AS DATE) AS thoiHan,
            CAST(NULL AS NVARCHAR2(50)) AS khamMat,
            CAST(NULL AS NVARCHAR2(50)) AS huyetAp,
            CAST(NULL AS NUMBER(5,2)) AS chieuCao,
            CAST(NULL AS NUMBER(5,2)) AS canNang
        FROM dual
        WHERE 1 = 0;

        OPEN p_images FOR
        SELECT CAST(NULL AS NVARCHAR2(300)) AS urlAnh
        FROM dual
        WHERE 1 = 0;
END;
/
CREATE OR REPLACE FUNCTION FN_HAS_VALID_HOSO_BY_USER_HANG (
    p_userId IN NUMBER,
    p_hangId IN NUMBER
) RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM HoSoThiSinh hs
         JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
    WHERE hv.userId = p_userId
      AND hs.hangId = p_hangId
      AND ADD_MONTHS(TRUNC(hs.ngayDangKy), 12) >= TRUNC(SYSDATE);

    RETURN v_count;
END;
/

CREATE OR REPLACE PROCEDURE SP_CREATE_HOSO (
    p_userId           IN NUMBER,
    p_hangId           IN NUMBER,
    p_loaiHoSo         IN NVARCHAR2,
    p_ghiChu           IN NVARCHAR2,
    p_hieuLuc          IN NVARCHAR2,
    p_thoiHan          IN DATE,
    p_khamMat          IN NVARCHAR2,
    p_huyetAp          IN NVARCHAR2,
    p_chieuCao         IN NUMBER,
    p_canNang          IN NUMBER,
    p_hoSoId           OUT NUMBER,
    p_khamSucKhoeId    OUT NUMBER,
    p_message          OUT NVARCHAR2
)
AS
    v_hocVienId     NUMBER;
    v_hoTen         NVARCHAR2(100);
    v_avatarUrl     NVARCHAR2(500);
    v_tenHang       NVARCHAR2(50);
    v_tenHoSo       NVARCHAR2(255);
    v_exists        NUMBER;
BEGIN
    p_hoSoId := NULL;
    p_khamSucKhoeId := NULL;
    p_message := NULL;

    SELECT hv.hocVienId, hv.hoTen, hv.avatarUrl
    INTO v_hocVienId, v_hoTen, v_avatarUrl
    FROM HocVien hv
    WHERE hv.userId = p_userId;

    SELECT hg.tenHang
    INTO v_tenHang
    FROM HangGplx hg
    WHERE hg.hangId = p_hangId;

    v_exists := FN_HAS_VALID_HOSO_BY_USER_HANG(p_userId, p_hangId);

    IF v_exists > 0 THEN
        p_message := N'Bạn đã có hồ sơ còn hạn cho hạng này. Vui lòng sử dụng hồ sơ hiện có.';
        RETURN;
    END IF;

    v_tenHoSo := p_loaiHoSo || N' - Hồ sơ hạng ' || v_tenHang || N' - ' || v_hoTen;

    INSERT INTO PhieuKhamSucKhoe (
        hieuLuc,
        thoiHan,
        khamMat,
        huyetAp,
        chieuCao,
        canNang,
        urlAnh
    )
    VALUES (
        p_hieuLuc,
        p_thoiHan,
        p_khamMat,
        p_huyetAp,
        p_chieuCao,
        p_canNang,
        v_avatarUrl
    )
    RETURNING khamSucKhoeId INTO p_khamSucKhoeId;

    INSERT INTO HoSoThiSinh (
        hocVienId,
        tenHoSo,
        loaiHoSo,
        ngayDangKy,
        trangThai,
        ghiChu,
        khamSucKhoeId,
        hangId
    )
    VALUES (
        v_hocVienId,
        v_tenHoSo,
        p_loaiHoSo,
        SYSDATE,
        N'Đang xử lý',
        CASE
            WHEN p_ghiChu IS NULL OR TRIM(p_ghiChu) = '' THEN NULL
            ELSE p_ghiChu
        END,
        p_khamSucKhoeId,
        p_hangId
    )
    RETURNING hoSoId INTO p_hoSoId;

    p_message := N'Tạo hồ sơ thành công';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_message := N'Không tìm thấy học viên hoặc hạng GPLX hợp lệ';
    WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20001, SQLERRM);
END;
/

CREATE OR REPLACE PROCEDURE SP_ADD_ANH_GKSK (
    p_khamSucKhoeId IN NUMBER,
    p_urlAnh        IN NVARCHAR2,
    p_message       OUT NVARCHAR2
)
AS
BEGIN
    INSERT INTO AnhGksk (
        khamSucKhoeId,
        urlAnh
    )
    VALUES (
        p_khamSucKhoeId,
        p_urlAnh
    );

    p_message := N'Thêm ảnh thành công';
EXCEPTION
    WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20002, SQLERRM);
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_HANG_GPLX (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT hangId, maHang, tenHang
    FROM HangGplx
    ORDER BY tenHang;
END;
/

CREATE OR REPLACE PROCEDURE SP_GET_HOCVIEN_NAME_BY_USER (
    p_userId IN NUMBER,
    p_hoTen OUT NVARCHAR2
)
AS
BEGIN
    SELECT hv.hoTen
    INTO p_hoTen
    FROM HocVien hv
    WHERE hv.userId = p_userId;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_hoTen := NULL;
END;
/
CREATE OR REPLACE PROCEDURE SP_CHECK_CREATE_HOSO_CONDITION (
    p_userId IN NUMBER,
    p_avatarUrl OUT NVARCHAR2,
    p_canCreate OUT NUMBER,
    p_missingFields OUT NVARCHAR2
)
AS
    v_hoTen NVARCHAR2(100);
    v_cccd NVARCHAR2(20);
    v_sdt NVARCHAR2(15);
    v_email NVARCHAR2(50);
    v_gioiTinh NVARCHAR2(10);
    v_avatar NVARCHAR2(500);
    v_missing NVARCHAR2(2000) := N'';
BEGIN
    SELECT hoTen, soCmndCccd, sdt, email, gioiTinh, avatarUrl
    INTO v_hoTen, v_cccd, v_sdt, v_email, v_gioiTinh, v_avatar
    FROM HocVien
    WHERE userId = p_userId;

    IF v_hoTen IS NULL OR TRIM(v_hoTen) = '' THEN
        v_missing := v_missing || N'Họ tên|';
    END IF;

    IF v_cccd IS NULL OR TRIM(v_cccd) = '' THEN
        v_missing := v_missing || N'CCCD|';
    END IF;

    IF v_sdt IS NULL OR TRIM(v_sdt) = '' THEN
        v_missing := v_missing || N'Số điện thoại|';
    END IF;

    IF v_email IS NULL OR TRIM(v_email) = '' THEN
        v_missing := v_missing || N'Email|';
    END IF;

    IF v_gioiTinh IS NULL OR TRIM(v_gioiTinh) = '' THEN
        v_missing := v_missing || N'Giới tính|';
    END IF;

    IF v_avatar IS NULL OR TRIM(v_avatar) = '' THEN
        v_missing := v_missing || N'Ảnh thẻ|';
    END IF;

    p_avatarUrl := v_avatar;

    IF v_missing IS NULL OR v_missing = '' THEN
        p_canCreate := 1;
        p_missingFields := NULL;
    ELSE
        p_canCreate := 0;
        p_missingFields := RTRIM(v_missing, '|');
    END IF;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_avatarUrl := NULL;
        p_canCreate := 0;
        p_missingFields := N'Học viên';
END;
/
CREATE OR REPLACE FUNCTION FN_CAN_EDIT_HOSO (
    p_userId IN NUMBER,
    p_hoSoId IN NUMBER
) RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM HoSoThiSinh hs
         JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
    WHERE hs.hoSoId = p_hoSoId
      AND hv.userId = p_userId
      AND ADD_MONTHS(TRUNC(hs.ngayDangKy), 12) >= TRUNC(SYSDATE)
      AND hs.trangThai IN (N'Đang xử lý', N'Bị loại');

    RETURN v_count;
END;
/
CREATE OR REPLACE FUNCTION FN_HAS_VALID_HOSO_OTHER_HANG (
    p_userId IN NUMBER,
    p_hangId IN NUMBER,
    p_hoSoId IN NUMBER
) RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM HoSoThiSinh hs
         JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
    WHERE hv.userId = p_userId
      AND hs.hangId = p_hangId
      AND hs.hoSoId <> p_hoSoId
      AND ADD_MONTHS(TRUNC(hs.ngayDangKy), 12) >= TRUNC(SYSDATE);

    RETURN v_count;
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_EDIT_HOSO_BY_USER (
    p_userId IN NUMBER,
    p_hoSoId IN NUMBER,
    p_info OUT SYS_REFCURSOR,
    p_images OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_info FOR
        SELECT hs.hoSoId,
               hv.hoTen,
               hv.soCmndCccd,
               hv.namSinh,
               hv.gioiTinh,
               hv.sdt,
               hv.email,
               hv.avatarUrl,
               hs.tenHoSo,
               hs.loaiHoSo,
               hs.ngayDangKy,
               hs.trangThai,
               hs.ghiChu,
               hs.hangId,
               hg.tenHang,
               pk.khamSucKhoeId,
               pk.hieuLuc,
               pk.thoiHan,
               pk.khamMat,
               pk.huyetAp,
               pk.chieuCao,
               pk.canNang,
               pk.urlAnh
        FROM HoSoThiSinh hs
             JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
             JOIN HangGplx hg ON hg.hangId = hs.hangId
             LEFT JOIN PhieuKhamSucKhoe pk ON pk.khamSucKhoeId = hs.khamSucKhoeId
        WHERE hs.hoSoId = p_hoSoId
          AND hv.userId = p_userId;

    OPEN p_images FOR
        SELECT ag.anhId,
               ag.urlAnh
        FROM HoSoThiSinh hs
             JOIN HocVien hv ON hv.hocVienId = hs.hocVienId
             JOIN AnhGksk ag ON ag.khamSucKhoeId = hs.khamSucKhoeId
        WHERE hs.hoSoId = p_hoSoId
          AND hv.userId = p_userId
        ORDER BY ag.anhId;
END;
/
CREATE OR REPLACE PROCEDURE SP_UPDATE_MY_HOSO (
    p_userId           IN NUMBER,
    p_hoSoId           IN NUMBER,
    p_hangId           IN NUMBER,
    p_loaiHoSo         IN NVARCHAR2,
    p_ghiChu           IN NVARCHAR2,
    p_hieuLuc          IN NVARCHAR2,
    p_thoiHan          IN DATE,
    p_khamMat          IN NVARCHAR2,
    p_huyetAp          IN NVARCHAR2,
    p_chieuCao         IN NUMBER,
    p_canNang          IN NUMBER,
    p_khamSucKhoeId    OUT NUMBER,
    p_message          OUT NVARCHAR2
)
AS
    v_hocVienId     NUMBER;
    v_hoTen         NVARCHAR2(100);
    v_avatarUrl     NVARCHAR2(500);
    v_tenHang       NVARCHAR2(50);
    v_tenHoSo       NVARCHAR2(255);
    v_canEdit       NUMBER;
    v_exists        NUMBER;
BEGIN
    p_khamSucKhoeId := NULL;
    p_message := NULL;

    v_canEdit := FN_CAN_EDIT_HOSO(p_userId, p_hoSoId);

    IF v_canEdit = 0 THEN
        p_message := N'Hồ sơ này không còn đủ điều kiện để chỉnh sửa.';
        RETURN;
    END IF;

    v_exists := FN_HAS_VALID_HOSO_OTHER_HANG(p_userId, p_hangId, p_hoSoId);

    IF v_exists > 0 THEN
        p_message := N'Bạn đã có hồ sơ còn hạn cho hạng này. Vui lòng sử dụng hồ sơ hiện có.';
        RETURN;
    END IF;

    SELECT hv.hocVienId, hv.hoTen, hv.avatarUrl
    INTO v_hocVienId, v_hoTen, v_avatarUrl
    FROM HocVien hv
    WHERE hv.userId = p_userId;

    SELECT hg.tenHang
    INTO v_tenHang
    FROM HangGplx hg
    WHERE hg.hangId = p_hangId;

    v_tenHoSo := p_loaiHoSo || N' - Hồ sơ hạng ' || v_tenHang || N' - ' || v_hoTen;

    UPDATE HoSoThiSinh
    SET tenHoSo = v_tenHoSo,
        loaiHoSo = p_loaiHoSo,
        ghiChu = CASE
                    WHEN p_ghiChu IS NULL OR TRIM(p_ghiChu) = '' THEN NULL
                    ELSE p_ghiChu
                 END,
        hangId = p_hangId
    WHERE hoSoId = p_hoSoId
      AND hocVienId = v_hocVienId;

    SELECT hs.khamSucKhoeId
    INTO p_khamSucKhoeId
    FROM HoSoThiSinh hs
    WHERE hs.hoSoId = p_hoSoId;

    UPDATE PhieuKhamSucKhoe
    SET hieuLuc = p_hieuLuc,
        thoiHan = p_thoiHan,
        khamMat = p_khamMat,
        huyetAp = p_huyetAp,
        chieuCao = p_chieuCao,
        canNang = p_canNang,
        urlAnh = v_avatarUrl
    WHERE khamSucKhoeId = p_khamSucKhoeId;

    p_message := N'Cập nhật hồ sơ thành công';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_message := N'Không tìm thấy hồ sơ hợp lệ để cập nhật';
    WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20011, SQLERRM);
END;
/
CREATE OR REPLACE PROCEDURE SP_DELETE_ANH_GKSK_BY_KHAMID (
    p_khamSucKhoeId IN NUMBER,
    p_message OUT NVARCHAR2
)
AS
BEGIN
    DELETE FROM AnhGksk
    WHERE khamSucKhoeId = p_khamSucKhoeId;

    p_message := N'Đã xóa ảnh cũ';
EXCEPTION
    WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20012, SQLERRM);
END;
/
CREATE OR REPLACE VIEW VW_HOSO_HOCVIEN_HIEULUC
AS
SELECT
    hs.hoSoId,
    hs.hocVienId,
    hs.tenHoSo,
    hs.hangId,
    hs.ngayDangKy,
    hs.trangThai,
    ADD_MONTHS(TRUNC(hs.ngayDangKy), 12) AS ngayHetHan,
    CASE
        WHEN hs.ngayDangKy IS NOT NULL
         AND ADD_MONTHS(TRUNC(hs.ngayDangKy), 12) >= TRUNC(SYSDATE)
        THEN 1
        ELSE 0
    END AS conHan
FROM HoSoThiSinh hs;
/
CREATE OR REPLACE PACKAGE PKG_KHOAHOC
AS
    PROCEDURE SP_GET_KHOAHOC_DANG_MO
    (
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_KHOAHOC_DETAIL
    (
        p_khoaHocId IN NUMBER,
        p_cursor    OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_HOSO_STATUS_INDEX
    (
        p_userId IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_CHECK_DANGKY
    (
        p_userId    IN NUMBER,
        p_khoaHocId IN NUMBER,
        p_cursor    OUT SYS_REFCURSOR
    );
END PKG_KHOAHOC;
/
CREATE OR REPLACE PACKAGE BODY PKG_KHOAHOC
AS
    PROCEDURE SP_GET_KHOAHOC_DANG_MO
    (
        p_cursor OUT SYS_REFCURSOR
    )
    AS
    BEGIN
        OPEN p_cursor FOR
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.diaDiem,
                kh.trangThai,
                hg.tenHang,
                hg.hocPhi
            FROM KhoaHoc kh
            JOIN HangGplx hg ON kh.hangId = hg.hangId
            WHERE kh.trangThai IN (N'Sắp khai giảng', N'Đang học')
            ORDER BY kh.ngayBatDau ASC;
    END SP_GET_KHOAHOC_DANG_MO;

    PROCEDURE SP_GET_KHOAHOC_DETAIL
    (
        p_khoaHocId IN NUMBER,
        p_cursor    OUT SYS_REFCURSOR
    )
    AS
    BEGIN
        OPEN p_cursor FOR
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.diaDiem,
                kh.trangThai,
                hg.hangId,
                hg.tenHang,
                hg.moTa,
                hg.loaiPhuongTien,
                hg.soCauHoi,
                hg.diemDat,
                hg.thoiGianTn,
                hg.hocPhi
            FROM KhoaHoc kh
            JOIN HangGplx hg ON kh.hangId = hg.hangId
            WHERE kh.khoaHocId = p_khoaHocId;
    END SP_GET_KHOAHOC_DETAIL;

    PROCEDURE SP_GET_HOSO_STATUS_INDEX
    (
        p_userId IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    )
    AS
    BEGIN
        OPEN p_cursor FOR
            WITH HOC_VIEN_USER AS
            (
                SELECT hv.hocVienId
                FROM HocVien hv
                WHERE hv.userId = p_userId
            ),
            HOSO_ALL AS
            (
                SELECT v.*
                FROM VW_HOSO_HOCVIEN_HIEULUC v
                JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = v.hocVienId
            ),
            TONG_HOP AS
            (
                SELECT
                    COUNT(*) AS tongHoSo,
                    SUM(CASE WHEN conHan = 1 THEN 1 ELSE 0 END) AS tongHoSoConHan,
                    SUM(CASE WHEN conHan = 1 AND trangThai = N'Đã duyệt' THEN 1 ELSE 0 END) AS tongHoSoDaDuyetConHan,
                    SUM(CASE WHEN conHan = 1 AND trangThai = N'Đang xử lý' THEN 1 ELSE 0 END) AS tongHoSoDangXuLyConHan,
                    SUM(CASE WHEN conHan = 0 THEN 1 ELSE 0 END) AS tongHoSoHetHan
                FROM HOSO_ALL
            )
            SELECT
                NVL(t.tongHoSo, 0) AS tongHoSo,
                NVL(t.tongHoSoConHan, 0) AS tongHoSoConHan,
                NVL(t.tongHoSoDaDuyetConHan, 0) AS tongHoSoDaDuyetConHan,
                NVL(t.tongHoSoDangXuLyConHan, 0) AS tongHoSoDangXuLyConHan,
                NVL(t.tongHoSoHetHan, 0) AS tongHoSoHetHan,
                CASE
                    WHEN NVL(t.tongHoSo, 0) = 0 THEN 1
                    WHEN NVL(t.tongHoSoConHan, 0) = 0 THEN 1
                    ELSE 0
                END AS showModal,
                CASE
                    WHEN NVL(t.tongHoSo, 0) = 0 THEN N'NO_PROFILE'
                    WHEN NVL(t.tongHoSoConHan, 0) = 0 THEN N'ALL_EXPIRED'
                    ELSE N'OK'
                END AS statusCode,
                CASE
                    WHEN NVL(t.tongHoSo, 0) = 0 THEN
                        N'Bạn hiện chưa có hồ sơ học viên. Để đăng ký khóa học và sử dụng đầy đủ chức năng của hệ thống, vui lòng tạo hồ sơ mới.'
                    WHEN NVL(t.tongHoSoConHan, 0) = 0 THEN
                        N'Hồ sơ hiện tại của bạn đã hết hiệu lực. Vui lòng tạo hồ sơ mới để tiếp tục đăng ký khóa học.'
                    ELSE
                        N'Hồ sơ hợp lệ.'
                END AS statusMessage
            FROM TONG_HOP t;
    END SP_GET_HOSO_STATUS_INDEX;

    PROCEDURE SP_CHECK_DANGKY
    (
        p_userId    IN NUMBER,
        p_khoaHocId IN NUMBER,
        p_cursor    OUT SYS_REFCURSOR
    )
    AS
    BEGIN
        OPEN p_cursor FOR
            WITH KHOA_HOC_TARGET AS
            (
                SELECT
                    kh.khoaHocId,
                    kh.tenKhoaHoc,
                    kh.ngayBatDau,
                    kh.ngayKetThuc,
                    kh.diaDiem,
                    kh.trangThai,
                    hg.hangId,
                    hg.tenHang,
                    hg.moTa,
                    hg.loaiPhuongTien,
                    hg.soCauHoi,
                    hg.diemDat,
                    hg.thoiGianTn,
                    hg.hocPhi
                FROM KhoaHoc kh
                JOIN HangGplx hg ON hg.hangId = kh.hangId
                WHERE kh.khoaHocId = p_khoaHocId
            ),
            HOC_VIEN_USER AS
            (
                SELECT hv.hocVienId
                FROM HocVien hv
                WHERE hv.userId = p_userId
            ),
            HO_SO_ALL AS
            (
                SELECT v.*
                FROM VW_HOSO_HOCVIEN_HIEULUC v
                JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = v.hocVienId
            ),
            THONG_KE_ALL AS
            (
                SELECT
                    COUNT(*) AS tongHoSo,
                    SUM(CASE WHEN conHan = 1 THEN 1 ELSE 0 END) AS tongHoSoConHan
                FROM HO_SO_ALL
            ),
            HO_SO_CUNG_HANG AS
            (
                SELECT
                    hs.hoSoId,
                    hs.tenHoSo,
                    hs.ngayDangKy,
                    hs.trangThai,
                    hs.ngayHetHan,
                    hs.conHan,
                    hs.hangId
                FROM HO_SO_ALL hs
                JOIN KHOA_HOC_TARGET kht ON kht.hangId = hs.hangId
            ),
            THONG_KE_CUNG_HANG AS
            (
                SELECT
                    COUNT(*) AS tongHoSoCungHang,
                    SUM(CASE WHEN conHan = 1 THEN 1 ELSE 0 END) AS tongHoSoCungHangConHan,
                    SUM(CASE WHEN conHan = 1 AND trangThai = N'Đã duyệt' THEN 1 ELSE 0 END) AS tongHoSoDaDuyetConHan,
                    SUM(CASE WHEN conHan = 1 AND trangThai = N'Đang xử lý' THEN 1 ELSE 0 END) AS tongHoSoDangXuLyConHan,
                    SUM(CASE WHEN conHan = 1 AND trangThai = N'Bị loại' THEN 1 ELSE 0 END) AS tongHoSoBiLoaiConHan,
                    SUM(CASE WHEN conHan = 0 THEN 1 ELSE 0 END) AS tongHoSoHetHan
                FROM HO_SO_CUNG_HANG
            ),
            HO_SO_PHU_HOP_DA_DUYET AS
            (
                SELECT *
                FROM
                (
                    SELECT
                        hs.hoSoId,
                        hs.tenHoSo,
                        hs.ngayDangKy,
                        hs.trangThai,
                        hs.ngayHetHan,
                        ROW_NUMBER() OVER
                        (
                            ORDER BY hs.ngayDangKy DESC, hs.hoSoId DESC
                        ) AS rn
                    FROM HO_SO_CUNG_HANG hs
                    WHERE hs.conHan = 1
                      AND hs.trangThai = N'Đã duyệt'
                )
                WHERE rn = 1
            ),
            DA_HOC_HANG AS
            (
                SELECT COUNT(*) AS soLanDaHoc
                FROM KetQuaHocTap kq
                JOIN HoSoThiSinh hs ON hs.hoSoId = kq.hoSoId
                JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = hs.hocVienId
                JOIN ChiTietKetQuaHocTap ct ON ct.ketQuaHocTapId = kq.ketQuaHocTapId
                JOIN KhoaHoc kh ON kh.khoaHocId = ct.khoaHocId
                JOIN KHOA_HOC_TARGET kht ON kht.hangId = kh.hangId
            ),
            KHOA_HOC_DA_DANG_KY AS
            (
                SELECT
                    kh.khoaHocId,
                    kh.tenKhoaHoc,
                    kh.ngayBatDau,
                    kh.ngayKetThuc,
                    kh.trangThai,
                    hg.hangId,
                    hg.tenHang,
                    ct.phieuId,
                    pt.ngayLap,
                    pt.ngayNop,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY kh.khoaHocId
                        ORDER BY pt.phieuId DESC
                    ) AS rn
                FROM HocVien hv
                JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
                JOIN ChiTietPhieuThanhToan ct ON ct.hoSoId = hs.hoSoId
                JOIN PhieuThanhToan pt ON pt.phieuId = ct.phieuId
                JOIN KetQuaHocTap kq ON kq.ketQuaHocTapId = ct.ketQuaHocTapId
                JOIN ChiTietKetQuaHocTap ct_kq ON ct_kq.ketQuaHocTapId = kq.ketQuaHocTapId
                JOIN KhoaHoc kh ON kh.khoaHocId = ct_kq.khoaHocId
                JOIN HangGplx hg ON hg.hangId = kh.hangId
                WHERE hv.userId = p_userId
                  AND pt.ngayNop IS NOT NULL
            ),
            KHOA_HOC_DA_DANG_KY_DISTINCT AS
            (
                SELECT *
                FROM KHOA_HOC_DA_DANG_KY
                WHERE rn = 1
            ),
            DA_DANG_KY_CHINH_KHOA_HOC AS
            (
                SELECT
                    COUNT(*) AS soLuongDangKy,
                    MAX(khdk.khoaHocId) AS khoaHocIdDaDangKy,
                    MAX(khdk.tenKhoaHoc) KEEP
                    (
                        DENSE_RANK LAST ORDER BY khdk.khoaHocId
                    ) AS tenKhoaHocDaDangKy
                FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
                JOIN KHOA_HOC_TARGET kht ON kht.khoaHocId = khdk.khoaHocId
            ),
            DANG_TRUNG_THOI_GIAN AS
            (
                SELECT
                    COUNT(*) AS soLuongTrung,
                    MAX(khdk.khoaHocId) AS khoaHocIdTrung,
                    MAX(khdk.tenKhoaHoc) KEEP
                    (
                        DENSE_RANK LAST ORDER BY khdk.khoaHocId
                    ) AS tenKhoaHocTrung,
                    MAX(khdk.ngayBatDau) KEEP
                    (
                        DENSE_RANK LAST ORDER BY khdk.khoaHocId
                    ) AS ngayBatDauTrung,
                    MAX(khdk.ngayKetThuc) KEEP
                    (
                        DENSE_RANK LAST ORDER BY khdk.khoaHocId
                    ) AS ngayKetThucTrung
                FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
                CROSS JOIN KHOA_HOC_TARGET kht
                WHERE khdk.ngayBatDau IS NOT NULL
                  AND khdk.ngayKetThuc IS NOT NULL
                  AND kht.ngayBatDau IS NOT NULL
                  AND kht.ngayKetThuc IS NOT NULL
                  AND khdk.ngayBatDau <= kht.ngayKetThuc
                  AND khdk.ngayKetThuc >= kht.ngayBatDau
            ),
            DA_DANG_KY_CUNG_HANG AS
            (
                SELECT
                    COUNT(*) AS soLuongCungHang,
                    MAX(khdk.khoaHocId) AS khoaHocIdCungHang,
                    MAX(khdk.tenKhoaHoc) KEEP
                    (
                        DENSE_RANK LAST ORDER BY khdk.khoaHocId
                    ) AS tenKhoaHocCungHang
                FROM KHOA_HOC_DA_DANG_KY_DISTINCT khdk
                JOIN KHOA_HOC_TARGET kht ON kht.hangId = khdk.hangId
                WHERE khdk.khoaHocId <> kht.khoaHocId
            )
            SELECT
                kht.khoaHocId,
                kht.tenKhoaHoc,
                kht.ngayBatDau,
                kht.ngayKetThuc,
                kht.diaDiem,
                kht.trangThai,
                kht.hangId,
                kht.tenHang,
                kht.moTa,
                kht.loaiPhuongTien,
                kht.soCauHoi,
                kht.diemDat,
                kht.thoiGianTn,
                kht.hocPhi,

                CASE WHEN kht.trangThai = N'Sắp khai giảng' THEN 1 ELSE 0 END AS isMoDangKy,

                NVL(ta.tongHoSo, 0) AS tongHoSo,
                NVL(ta.tongHoSoConHan, 0) AS tongHoSoConHan,
                NVL(tk.tongHoSoCungHang, 0) AS tongHoSoCungHang,
                NVL(tk.tongHoSoDaDuyetConHan, 0) AS tongHoSoDaDuyetConHan,
                NVL(tk.tongHoSoDangXuLyConHan, 0) AS tongHoSoDangXuLyConHan,
                NVL(tk.tongHoSoBiLoaiConHan, 0) AS tongHoSoBiLoaiConHan,
                NVL(tk.tongHoSoHetHan, 0) AS tongHoSoHetHan,

                CASE
                    WHEN hsp.hoSoId IS NOT NULL THEN 1
                    ELSE 0
                END AS hasHoSoPhuHop,

                CASE
                    WHEN NVL(tk.tongHoSoDangXuLyConHan, 0) > 0
                         AND hsp.hoSoId IS NULL THEN 1
                    ELSE 0
                END AS hasHoSoChuaDuyet,

                NVL(hsp.hoSoId, 0) AS hoSoIdPhuHop,
                NVL(hsp.tenHoSo, N'') AS tenHoSoPhuHop,
                hsp.ngayDangKy AS ngayDangKyHoSo,
                NVL(hsp.trangThai, N'') AS trangThaiHoSo,

                CASE
                    WHEN NVL(dhh.soLanDaHoc, 0) > 0 THEN 1
                    ELSE 0
                END AS daTungHocHang,

                NVL(dhh.soLanDaHoc, 0) AS soLanDaHocHang,

                CASE
                    WHEN NVL(ddkckh.soLuongDangKy, 0) > 0 THEN 1
                    ELSE 0
                END AS daDangKyChinhKhoaHoc,

                NVL(ddkckh.khoaHocIdDaDangKy, 0) AS khoaHocIdDaDangKy,
                NVL(ddkckh.tenKhoaHocDaDangKy, N'') AS tenKhoaHocDaDangKy,

                CASE
                    WHEN NVL(dttg.soLuongTrung, 0) > 0 THEN 1
                    ELSE 0
                END AS biTrungThoiGianHoc,

                NVL(dttg.khoaHocIdTrung, 0) AS khoaHocIdTrungThoiGian,
                NVL(dttg.tenKhoaHocTrung, N'') AS tenKhoaHocTrungThoiGian,
                dttg.ngayBatDauTrung AS ngayBatDauTrungThoiGian,
                dttg.ngayKetThucTrung AS ngayKetThucTrungThoiGian,

                CASE
                    WHEN NVL(ddkch.soLuongCungHang, 0) > 0 THEN 1
                    ELSE 0
                END AS daTungDangKyCungHang,

                NVL(ddkch.khoaHocIdCungHang, 0) AS khoaHocIdCungHangGanNhat,
                NVL(ddkch.tenKhoaHocCungHang, N'') AS tenKhoaHocCungHangGanNhat,

                CASE
                    WHEN kht.trangThai <> N'Sắp khai giảng' THEN N'COURSE_NOT_OPEN'
                    WHEN NVL(ta.tongHoSo, 0) = 0 THEN N'NO_PROFILE'
                    WHEN NVL(ta.tongHoSoConHan, 0) = 0 THEN N'ALL_PROFILE_EXPIRED'
                    WHEN NVL(tk.tongHoSoCungHang, 0) = 0 THEN N'NO_PROFILE_FOR_HANG'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0
                         AND NVL(ddkckh.soLuongDangKy, 0) > 0 THEN N'ALREADY_REGISTERED'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0
                         AND NVL(dttg.soLuongTrung, 0) > 0 THEN N'TIME_CONFLICT'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0 THEN N'ELIGIBLE'
                    WHEN NVL(tk.tongHoSoDangXuLyConHan, 0) > 0 THEN N'PENDING_APPROVAL'
                    WHEN NVL(tk.tongHoSoHetHan, 0) > 0 THEN N'PROFILE_EXPIRED'
                    WHEN NVL(tk.tongHoSoBiLoaiConHan, 0) > 0 THEN N'PROFILE_REJECTED'
                    ELSE N'NO_PROFILE_FOR_HANG'
                END AS statusCode,

                CASE
                    WHEN kht.trangThai <> N'Sắp khai giảng' THEN
                        N'Khóa học này hiện chưa mở đăng ký.'
                    WHEN NVL(ta.tongHoSo, 0) = 0 THEN
                        N'Bạn chưa có hồ sơ học viên. Vui lòng tạo hồ sơ mới trước khi đăng ký khóa học.'
                    WHEN NVL(ta.tongHoSoConHan, 0) = 0 THEN
                        N'Hồ sơ hiện tại của bạn đã hết hiệu lực. Vui lòng tạo hồ sơ mới để tiếp tục đăng ký khóa học.'
                    WHEN NVL(tk.tongHoSoCungHang, 0) = 0 THEN
                        N'Bạn chưa có hồ sơ hạng ' || kht.tenHang || N'. Vui lòng tạo hồ sơ đúng hạng để đăng ký khóa học này.'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0
                         AND NVL(ddkckh.soLuongDangKy, 0) > 0 THEN
                        N'Bạn đã đăng ký khóa học "' || NVL(ddkckh.tenKhoaHocDaDangKy, kht.tenKhoaHoc) || N'" rồi.'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0
                         AND NVL(dttg.soLuongTrung, 0) > 0 THEN
                        N'Bạn đã có khóa học "' || NVL(dttg.tenKhoaHocTrung, N'') || N'" bị trùng thời gian học, nên chưa thể đăng ký thêm khóa học này.'
                    WHEN NVL(tk.tongHoSoDaDuyetConHan, 0) > 0 THEN
                        N'Đủ điều kiện đăng ký khóa học.'
                    WHEN NVL(tk.tongHoSoDangXuLyConHan, 0) > 0 THEN
                        N'Bạn đã có hồ sơ hạng ' || kht.tenHang || N' đang chờ duyệt. Vui lòng đợi quản trị viên xác nhận trước khi đăng ký khóa học.'
                    WHEN NVL(tk.tongHoSoHetHan, 0) > 0 THEN
                        N'Hồ sơ hạng ' || kht.tenHang || N' của bạn đã hết hiệu lực. Vui lòng tạo hồ sơ mới để tiếp tục đăng ký.'
                    WHEN NVL(tk.tongHoSoBiLoaiConHan, 0) > 0 THEN
                        N'Hồ sơ hạng ' || kht.tenHang || N' của bạn hiện không đủ điều kiện sử dụng. Vui lòng tạo hồ sơ mới hoặc liên hệ quản trị viên để được hỗ trợ.'
                    ELSE
                        N'Bạn chưa đủ điều kiện đăng ký khóa học này.'
                END AS statusMessage,

                CASE
                    WHEN kht.trangThai = N'Sắp khai giảng'
                     AND NVL(tk.tongHoSoDaDuyetConHan, 0) > 0
                     AND NVL(ddkckh.soLuongDangKy, 0) = 0
                     AND NVL(dttg.soLuongTrung, 0) = 0
                    THEN 1
                    ELSE 0
                END AS coTheDangKy
            FROM KHOA_HOC_TARGET kht
            LEFT JOIN THONG_KE_ALL ta ON 1 = 1
            LEFT JOIN THONG_KE_CUNG_HANG tk ON 1 = 1
            LEFT JOIN HO_SO_PHU_HOP_DA_DUYET hsp ON 1 = 1
            LEFT JOIN DA_HOC_HANG dhh ON 1 = 1
            LEFT JOIN DA_DANG_KY_CHINH_KHOA_HOC ddkckh ON 1 = 1
            LEFT JOIN DANG_TRUNG_THOI_GIAN dttg ON 1 = 1
            LEFT JOIN DA_DANG_KY_CUNG_HANG ddkch ON 1 = 1;
    END SP_CHECK_DANGKY;
END PKG_KHOAHOC;
/
-- ==================================================
-- ADMIN QUẢN LÝ
-- ==================================================

-- KHÓA HỌC
CREATE OR REPLACE VIEW VW_KHOAHOC AS
SELECT 
    kh.khoaHocId,
    kh.tenKhoaHoc,
    kh.hangId,
    hg.tenHang,
    kh.ngayBatDau,
    kh.ngayKetThuc,
    kh.diaDiem,
    kh.trangThai
FROM KhoaHoc kh
JOIN HangGplx hg ON kh.hangId = hg.hangId;
/
CREATE OR REPLACE PACKAGE PKG_ADMINKHOAHOC AS
    TYPE REF_CURSOR IS REF CURSOR;

    PROCEDURE GET_LIST_KHOAHOC(
        p_keyword   IN NVARCHAR2,
        p_hangId    IN NUMBER,
        p_trangThai IN NVARCHAR2,
        p_page      IN NUMBER,
        p_pageSize  IN NUMBER,
        p_total     OUT NUMBER,
        p_cursor    OUT REF_CURSOR
    );

    PROCEDURE GET_DETAIL_KHOAHOC(
        p_khoaHocId IN NUMBER,
        p_cursor    OUT REF_CURSOR
    );

    PROCEDURE INSERT_KHOAHOC(
        p_hangId       IN NUMBER,
        p_tenKhoaHoc   IN NVARCHAR2,
        p_ngayBatDau   IN DATE,
        p_ngayKetThuc  IN DATE,
        p_diaDiem      IN NVARCHAR2,
        p_khoaHocId    OUT NUMBER
    );

    PROCEDURE UPDATE_KHOAHOC(
        p_khoaHocId    IN NUMBER,
        p_hangId       IN NUMBER,
        p_tenKhoaHoc   IN NVARCHAR2,
        p_ngayBatDau   IN DATE,
        p_ngayKetThuc  IN DATE,
        p_diaDiem      IN NVARCHAR2
    );
END PKG_ADMINKHOAHOC;
/
CREATE OR REPLACE PACKAGE BODY PKG_ADMINKHOAHOC AS

PROCEDURE GET_LIST_KHOAHOC(
    p_keyword   IN NVARCHAR2,
    p_hangId    IN NUMBER,
    p_trangThai IN NVARCHAR2,
    p_page      IN NUMBER,
    p_pageSize  IN NUMBER,
    p_total     OUT NUMBER,
    p_cursor    OUT REF_CURSOR
)
AS
    v_start_row NUMBER;
    v_end_row   NUMBER;
BEGIN
    v_start_row := ((p_page - 1) * p_pageSize) + 1;
    v_end_row   := p_page * p_pageSize;

    SELECT COUNT(*)
    INTO p_total
    FROM VW_KHOAHOC
    WHERE (p_keyword IS NULL OR LOWER(tenKhoaHoc) LIKE LOWER('%' || p_keyword || '%'))
      AND (p_hangId IS NULL OR hangId = p_hangId)
      AND (p_trangThai IS NULL OR trangThai = p_trangThai);

    OPEN p_cursor FOR
        SELECT *
        FROM
        (
            SELECT ROWNUM AS STT, x.*
            FROM
            (
                SELECT 
                    khoaHocId,
                    tenKhoaHoc,
                    hangId,
                    tenHang,
                    ngayBatDau,
                    ngayKetThuc,
                    diaDiem,
                    trangThai
                FROM VW_KHOAHOC
                WHERE (p_keyword IS NULL OR LOWER(tenKhoaHoc) LIKE LOWER('%' || p_keyword || '%'))
                  AND (p_hangId IS NULL OR hangId = p_hangId)
                  AND (p_trangThai IS NULL OR trangThai = p_trangThai)
                ORDER BY ngayBatDau DESC, khoaHocId DESC
            ) x
            WHERE ROWNUM <= v_end_row
        )
        WHERE STT >= v_start_row;
END GET_LIST_KHOAHOC;

PROCEDURE GET_DETAIL_KHOAHOC(
    p_khoaHocId IN NUMBER,
    p_cursor    OUT REF_CURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            khoaHocId,
            tenKhoaHoc,
            hangId,
            tenHang,
            ngayBatDau,
            ngayKetThuc,
            diaDiem,
            trangThai
        FROM VW_KHOAHOC
        WHERE khoaHocId = p_khoaHocId;
END GET_DETAIL_KHOAHOC;

PROCEDURE INSERT_KHOAHOC(
    p_hangId       IN NUMBER,
    p_tenKhoaHoc   IN NVARCHAR2,
    p_ngayBatDau   IN DATE,
    p_ngayKetThuc  IN DATE,
    p_diaDiem      IN NVARCHAR2,
    p_khoaHocId    OUT NUMBER
)
AS
BEGIN
    p_khoaHocId := SEQ_KHOAHOC.NEXTVAL;

    INSERT INTO KhoaHoc(
        khoaHocId,
        hangId,
        tenKhoaHoc,
        ngayBatDau,
        ngayKetThuc,
        diaDiem
    )
    VALUES(
        p_khoaHocId,
        p_hangId,
        p_tenKhoaHoc,
        p_ngayBatDau,
        p_ngayKetThuc,
        p_diaDiem
    );
END INSERT_KHOAHOC;

PROCEDURE UPDATE_KHOAHOC(
    p_khoaHocId    IN NUMBER,
    p_hangId       IN NUMBER,
    p_tenKhoaHoc   IN NVARCHAR2,
    p_ngayBatDau   IN DATE,
    p_ngayKetThuc  IN DATE,
    p_diaDiem      IN NVARCHAR2
)
AS
BEGIN
    UPDATE KhoaHoc
    SET hangId      = p_hangId,
        tenKhoaHoc  = p_tenKhoaHoc,
        ngayBatDau  = p_ngayBatDau,
        ngayKetThuc = p_ngayKetThuc,
        diaDiem     = p_diaDiem
    WHERE khoaHocId = p_khoaHocId;
END UPDATE_KHOAHOC;

END PKG_ADMINKHOAHOC;
/
-- KỲ THI
CREATE OR REPLACE VIEW VW_USER_KYTHI AS
WITH HV AS (
    SELECT hocVienId, hoTen
    FROM HocVien
),
HS AS (
    SELECT hs.hoSoId, hs.hocVienId, hs.hangId, hg.maHang, hg.hocPhi
    FROM HoSoThiSinh hs
    JOIN HangGplx hg ON hg.hangId = hs.hangId
),
KQHT AS (
    SELECT kq.*
    FROM KetQuaHocTap kq
),
CTKQHT AS (
    SELECT 
        ctkq.ketQuaHocTapId,
        CASE 
            WHEN NVL(lyThuyetKq,0)=1 
             AND NVL(saHinhKq,0)=1 
             AND NVL(duongTruongKq,0)=1 
             AND NVL(moPhongKq,0)=1
            THEN 1 ELSE 0 END AS DAT_FULL
    FROM ChiTietKetQuaHocTap ctkq
)
SELECT
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi,
    hs.hoSoId,
    hs.hocVienId,
    hs.maHang,
    hs.hocPhi,

    NVL(kq.DU_DK_THITOTNGHIEP,0) AS duDKTotNghiep,
    NVL(kq.DU_DK_THISATHACH,0) AS duDKSatHach,
    NVL(kq.DAUTOTNGHIEP,0) AS dauTotNghiep,

    NVL(ct.DAT_FULL,0) AS daHoanThanhTatCa,

    CASE
        WHEN NVL(ct.DAT_FULL,0)=1 THEN 0
        WHEN NVL(kq.DU_DK_THISATHACH,0)=1 THEN 1
        WHEN NVL(kq.DU_DK_THITOTNGHIEP,0)=1 THEN 1
        ELSE 0
    END AS coTheDangKy

FROM KyThi kt
JOIN HS 
    ON HS.maHang = REGEXP_SUBSTR(kt.tenKyThi, '\(([^)]+)\)$',1,1,NULL,1)
LEFT JOIN KQHT kq ON kq.hoSoId = hs.hoSoId
LEFT JOIN CTKQHT ct ON ct.ketQuaHocTapId = kq.ketQuaHocTapId;
/
CREATE OR REPLACE PROCEDURE GET_KYTHI_FOR_USER(
    p_userId NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_cursor FOR
    SELECT *
    FROM VW_USER_KYTHI v
    JOIN HocVien hv ON hv.hocVienId = v.hocVienId
    WHERE hv.userId = p_userId
    ORDER BY v.kyThiId DESC;
END;
/
CREATE OR REPLACE PROCEDURE DANGKY_KYTHI_USER(
    p_userId NUMBER,
    p_kyThiId NUMBER
)
IS
    v_hocVienId NUMBER;
    v_hoTen NVARCHAR2(100);
    v_hoSoId NUMBER;
    v_maHang NVARCHAR2(20);
    v_hocPhi NUMBER;

    v_duDKTot NUMBER;
    v_duDKSat NUMBER;
    v_dauTN NUMBER;
    v_full NUMBER;

    v_kyThiTot NUMBER;
    v_kyThiSat NUMBER;

    v_tenTot NVARCHAR2(255);
    v_tenSat NVARCHAR2(255);

    v_phi NUMBER;

    v_phieuId NUMBER;
BEGIN
    -- Lấy học viên
    SELECT hocVienId, hoTen INTO v_hocVienId, v_hoTen
    FROM HocVien WHERE userId = p_userId;

    -- Lấy thông tin kỳ thi
    SELECT 
        REGEXP_SUBSTR(tenKyThi,'\(([^)]+)\)$',1,1,NULL,1)
    INTO v_maHang
    FROM KyThi WHERE kyThiId = p_kyThiId;

    -- Lấy hồ sơ + học phí
    SELECT hs.hoSoId, hg.hocPhi
    INTO v_hoSoId, v_hocPhi
    FROM HoSoThiSinh hs
    JOIN HangGplx hg ON hg.hangId = hs.hangId
    WHERE hs.hocVienId = v_hocVienId
      AND hg.maHang = v_maHang
      AND ROWNUM = 1;

    -- Lấy kết quả học tập
    SELECT 
        NVL(DU_DK_THITOTNGHIEP,0),
        NVL(DU_DK_THISATHACH,0),
        NVL(DAUTOTNGHIEP,0)
    INTO v_duDKTot, v_duDKSat, v_dauTN
    FROM KetQuaHocTap
    WHERE hoSoId = v_hoSoId;

    -- Check full pass
    SELECT CASE 
        WHEN NVL(lyThuyetKq,0)=1 AND NVL(saHinhKq,0)=1 
         AND NVL(duongTruongKq,0)=1 AND NVL(moPhongKq,0)=1
        THEN 1 ELSE 0 END
    INTO v_full
    FROM ChiTietKetQuaHocTap
    WHERE ketQuaHocTapId = (
        SELECT ketQuaHocTapId FROM KetQuaHocTap WHERE hoSoId = v_hoSoId
    );

    -- Nếu đã hoàn thành → chặn
    IF v_full = 1 THEN
        RAISE_APPLICATION_ERROR(-20020, 'Da hoan thanh ky thi');
    END IF;

    -- Nếu chưa đủ điều kiện tốt nghiệp → chặn
    IF v_duDKTot = 0 THEN
        RAISE_APPLICATION_ERROR(-20021, 'Chua du dieu kien thi tot nghiep');
    END IF;

    -- Tìm 2 kỳ thi
    SELECT kyThiId, tenKyThi INTO v_kyThiTot, v_tenTot
    FROM KyThi
    WHERE REGEXP_SUBSTR(tenKyThi,'\(([^)]+)\)$',1,1,NULL,1)=v_maHang
      AND loaiKyThi = N'Tốt nghiệp'
      AND ROWNUM=1;

    SELECT kyThiId, tenKyThi INTO v_kyThiSat, v_tenSat
    FROM KyThi
    WHERE REGEXP_SUBSTR(tenKyThi,'\(([^)]+)\)$',1,1,NULL,1)=v_maHang
      AND loaiKyThi = N'Sát hạch'
      AND ROWNUM=1;

    v_phi := v_hocPhi / 3;

    -- CASE 1: đủ sát hạch → chỉ đăng ký 1
    IF v_duDKSat = 1 THEN

        INSERT INTO ChiTietDangKyThi
        VALUES (v_kyThiSat, v_hoSoId, SYSTIMESTAMP, NULL);

        INSERT INTO PhieuThanhToan(tenPhieu, ngayLap, tongTien)
        VALUES (v_tenSat || ' _ ' || v_hoTen, SYSTIMESTAMP, v_phi)
        RETURNING phieuId INTO v_phieuId;

        INSERT INTO ChiTietPhieuThanhToan
        VALUES (v_hoSoId, v_phieuId, N'Lệ phí thi', NULL, NULL);

    ELSE
    -- CASE 2: chỉ đủ tốt nghiệp → đăng ký cả 2

        INSERT INTO ChiTietDangKyThi
        VALUES (v_kyThiTot, v_hoSoId, SYSTIMESTAMP, NULL);

        INSERT INTO ChiTietDangKyThi
        VALUES (v_kyThiSat, v_hoSoId, SYSTIMESTAMP, NULL);

        INSERT INTO PhieuThanhToan(tenPhieu, ngayLap, tongTien)
        VALUES (v_tenTot || ' _ ' || v_hoTen, SYSTIMESTAMP, v_phi)
        RETURNING phieuId INTO v_phieuId;

        INSERT INTO ChiTietPhieuThanhToan
        VALUES (v_hoSoId, v_phieuId, N'Lệ phí thi', NULL, NULL);

        INSERT INTO PhieuThanhToan(tenPhieu, ngayLap, tongTien)
        VALUES (v_tenSat || ' _ ' || v_hoTen, SYSTIMESTAMP, v_phi)
        RETURNING phieuId INTO v_phieuId;

        INSERT INTO ChiTietPhieuThanhToan
        VALUES (v_hoSoId, v_phieuId, N'Lệ phí thi', NULL, NULL);

    END IF;

END;
/










COMMIT;