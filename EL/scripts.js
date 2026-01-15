// === 前端脚本：调用后端 API 与基本校验 ===

const API_BASE = "http://localhost:5000/api";

function getToken() {
  return localStorage.getItem("token");
}
function getUser() {
  const u = localStorage.getItem("user");
  return u ? JSON.parse(u) : null;
}
function authHeader() {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}
async function fetchJson(url, options = {}) {
  const res = await fetch(url, options);
  const contentType = res.headers.get("content-type");
  // 先获取响应文本
  const text = await res.text();

  if (!res.ok) {
    throw new Error(text || "请求失败");
  }

  // 检查响应是否有内容
  if (!text || text.trim().length === 0) {
    return null; // 返回null表示成功但无响应体
  }

  // 如果是JSON格式，解析JSON
  if (contentType && contentType.includes("application/json")) {
    try {
      return JSON.parse(text);
    } catch (e) {
      // 如果解析失败，返回文本
      return text;
    }
  }
  // 否则返回文本
  return text;
}

// 检查登录状态
function checkAuth() {
  const token = getToken();
  const user = getUser();
  return token && user;
}

// 要求登录，如果未登录则跳转到登录页
function requireAuth() {
  if (!checkAuth()) {
    alert("请先登录");
    window.location.href = "login.html";
    return false;
  }
  return true;
}

// 渲染导航登录状态
function renderNav() {
  const loginLink = document.getElementById("loginLink");
  if (!loginLink) return;
  const user = getUser();
  if (user) {
    loginLink.innerHTML = `<a href="#">欢迎 ${user.nickName} (退出)</a>`;
    loginLink.querySelector("a").addEventListener("click", () => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      location.reload();
    });
  }
}

// ------------ 商品渲染 -------------
// 使用与商品类别匹配的 Unsplash 图片，固定尺寸裁剪
const demoProducts = [
  // 手机类 (3个)
  {
    id: 102,
    name: "旗舰手机 X1",
    price: 3999,
    cat: "手机",
    img: "./phone1.png",
  },
  {
    id: 105,
    name: "5G 智能手机 Pro Max",
    price: 5299,
    cat: "手机",
    img: "./phone2.png",
  },
  {
    id: 106,
    name: "青春版手机 Lite",
    price: 1999,
    cat: "手机",
    img: "./phone3.png",
  },
  // 平板类 (2个)
  {
    id: 107,
    name: "iPad Pro 12.9寸",
    price: 6799,
    cat: "平板",
    img: "./pad1.png",
  },
  {
    id: 108,
    name: "安卓平板 Air",
    price: 2499,
    cat: "平板",
    img: "./pad2.png",
  },
  // 笔记本类 (2个)
  {
    id: 104,
    name: "轻薄笔记本 Pro",
    price: 5999,
    cat: "笔记本",
    img: "./comp.png",
  },
  {
    id: 109,
    name: "游戏本 RTX 4060",
    price: 8999,
    cat: "笔记本",
    img: "./comp2.jpg",
  },
  // 智能穿戴类 (2个)
  {
    id: 103,
    name: "智能手表 S2",
    price: 899,
    cat: "智能穿戴",
    img: "./watch.png",
  },
  {
    id: 110,
    name: "运动手环 Pro",
    price: 399,
    cat: "智能穿戴",
    img: "./watch1.png",
  },
  // 智能家居类 (3个)
  {
    id: 111,
    name: "智能音箱 Echo",
    price: 599,
    cat: "智能家居",
    img: "./voice1.png",
  },

  // 配件类 (3个)

  {
    id: 115,
    name: "蓝牙耳机 AirPods",
    price: 1299,
    cat: "配件",
    img: "./voice.png",
  },
];
function renderProducts(filterCat = "all") {
  const grid = document.getElementById("productGrid");
  if (!grid) return;
  grid.innerHTML = "";
  const isAll = !filterCat || filterCat === "all";
  const filtered = demoProducts.filter((p) => isAll || p.cat === filterCat);
  console.log(
    `渲染商品: 分类=${filterCat}, 总数=${demoProducts.length}, 筛选后=${filtered.length}`
  );
  filtered.forEach((p) => {
    const card = document.createElement("article");
    card.className = "product-card";
    card.innerHTML = `
        <img src="${p.img}" alt="${p.name}">
        <div class="info">
          <h3>${p.name}</h3>
          <footer>
            <span class="tag">${p.cat}</span>
            <p>¥${p.price}</p>
          </footer>
          <button class="action-btn" data-id="${p.id}">加入购物车</button>
        </div>`;
    card.addEventListener("click", (e) => {
      if (e.target.tagName !== "BUTTON") {
        location.href = `detail.html?id=${p.id}`;
      }
    });
    card.querySelector("button").addEventListener("click", (e) => {
      e.stopPropagation();
      addToCart(p.id, 1);
    });
    grid.appendChild(card);
  });
}
async function addToCart(pid, qty) {
  // 检查登录状态
  if (!checkAuth()) {
    alert("请先登录");
    window.location.href = "login.html";
    return;
  }

  try {
    const result = await fetchJson(`${API_BASE}/cart/add`, {
      method: "POST",
      headers: { ...authHeader(), "Content-Type": "application/json" },
      body: JSON.stringify({ productId: pid, quantity: qty }),
    });
    // result可能是null（空响应），这是正常的
    alert("已加入购物车");
  } catch (err) {
    if (err.message.includes("401") || err.message.includes("Unauthorized")) {
      alert("登录已过期，请重新登录");
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "login.html";
    } else {
      alert("加入购物车失败：" + err.message);
    }
  }
}

// ------------ 购物车渲染 -------------
async function loadCart() {
  const cartBody = document.getElementById("cartBody");
  const cartTotalEl = document.getElementById("cartTotal");
  if (!cartBody) return;

  // 检查登录状态
  if (!checkAuth()) {
    alert("请先登录");
    window.location.href = "login.html";
    return;
  }

  try {
    const items = await fetchJson(`${API_BASE}/cart`, {
      headers: authHeader(),
    });
    cartBody.innerHTML = "";
    let total = 0;

    if (items.length === 0) {
      cartBody.innerHTML = `<tr><td colspan="5" style="text-align:center;padding:40px;">购物车为空</td></tr>`;
      if (cartTotalEl) cartTotalEl.textContent = `合计：¥0.00`;
      return;
    }

    items.forEach((it) => {
      const sub = it.price * it.quantity;
      total += sub;
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td style="text-align:left; padding-left:20px;">${it.name}</td>
        <td>¥${it.price.toFixed(2)}</td>
        <td>
          <div class="quantity-control">
            <button class="action-btn" data-type="dec" data-id="${
              it.productId
            }">−</button>
            <span>${it.quantity}</span>
            <button class="action-btn" data-type="inc" data-id="${
              it.productId
            }">+</button>
          </div>
        </td>
        <td>¥${sub.toFixed(2)}</td>
        <td><button class="action-btn" data-type="del" data-id="${
          it.productId
        }">删除</button></td>`;
      cartBody.appendChild(tr);
    });
    if (cartTotalEl) cartTotalEl.textContent = `合计：¥${total.toFixed(2)}`;

    // 绑定事件
    cartBody.querySelectorAll("button[data-id]").forEach((btn) => {
      btn.addEventListener("click", async () => {
        if (!checkAuth()) {
          alert("请先登录");
          window.location.href = "login.html";
          return;
        }
        const pid = Number(btn.getAttribute("data-id"));
        const type = btn.getAttribute("data-type");
        try {
          if (type === "del") {
            await fetchJson(`${API_BASE}/cart/${pid}`, {
              method: "DELETE",
              headers: authHeader(),
            });
          } else {
            const delta = type === "inc" ? 1 : -1;
            await fetchJson(`${API_BASE}/cart/add`, {
              method: "POST",
              headers: { ...authHeader(), "Content-Type": "application/json" },
              body: JSON.stringify({ productId: pid, quantity: delta }),
            });
          }
          loadCart();
        } catch (err) {
          alert("操作失败：" + err.message);
        }
      });
    });
  } catch (err) {
    if (err.message.includes("401") || err.message.includes("Unauthorized")) {
      alert("登录已过期，请重新登录");
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "login.html";
    } else {
      alert("加载购物车失败：" + err.message);
    }
  }
}

// ------------ 订单渲染 -------------
async function loadOrders() {
  const orderBody = document.querySelector("#orderBody");
  if (!orderBody) return;

  // 检查登录状态
  if (!checkAuth()) {
    alert("请先登录");
    window.location.href = "login.html";
    return;
  }

  try {
    const orders = await fetchJson(`${API_BASE}/orders`, {
      headers: authHeader(),
    });
    orderBody.innerHTML = "";

    if (orders.length === 0) {
      orderBody.innerHTML = `<tr><td colspan="5" style="text-align:center;padding:40px;">暂无订单</td></tr>`;
      return;
    }

    orders.forEach((order) => {
      const tr = document.createElement("tr");
      const date = new Date(order.createdAt).toLocaleString("zh-CN");
      tr.innerHTML = `
        <td>${order.orderNumber}</td>
        <td>${date}</td>
        <td>¥${order.totalAmount.toFixed(2)}</td>
        <td>${order.status}</td>
        <td><button class="action-btn" data-order-id="${
          order.orderId
        }">查看</button></td>`;
      orderBody.appendChild(tr);

      // 绑定查看按钮事件
      tr.querySelector("button").addEventListener("click", async () => {
        try {
          const detail = await fetchJson(
            `${API_BASE}/orders/${order.orderId}`,
            { headers: authHeader() }
          );
          let itemsText = "订单详情：\n\n";
          detail.items.forEach((item) => {
            itemsText += `${item.productName} x${
              item.quantity
            } - ¥${item.price.toFixed(2)}\n`;
          });
          itemsText += `\n总计：¥${detail.totalAmount.toFixed(2)}`;
          alert(itemsText);
        } catch (err) {
          alert("加载订单详情失败：" + err.message);
        }
      });
    });
  } catch (err) {
    if (err.message.includes("401") || err.message.includes("Unauthorized")) {
      alert("登录已过期，请重新登录");
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "login.html";
    } else {
      alert("加载订单失败：" + err.message);
    }
  }
}

// ------------ 创建订单 -------------
async function createOrder() {
  if (!checkAuth()) {
    alert("请先登录");
    window.location.href = "login.html";
    return;
  }

  try {
    const result = await fetchJson(`${API_BASE}/orders/create`, {
      method: "POST",
      headers: authHeader(),
    });
    alert("订单创建成功！订单号：" + result.orderNumber);
    // 刷新购物车和订单页面
    if (document.getElementById("cartBody")) {
      loadCart();
    }
    if (document.getElementById("orderBody")) {
      loadOrders();
    }
    // 如果是从购物车页面创建订单，可以跳转到订单页面
    if (window.location.pathname.includes("cart.html")) {
      window.location.href = "order.html";
    }
  } catch (err) {
    if (err.message.includes("401") || err.message.includes("Unauthorized")) {
      alert("登录已过期，请重新登录");
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "login.html";
    } else {
      alert("创建订单失败：" + err.message);
    }
  }
}

// ------------ 页面路由判断 -------------

document.addEventListener("DOMContentLoaded", () => {
  renderNav();

  // 为购物车和订单链接添加登录检查
  const cartLink = document.querySelector('a[href="cart.html"]');
  const orderLink = document.querySelector('a[href="order.html"]');

  if (cartLink && !cartLink.hasAttribute("data-checked")) {
    cartLink.setAttribute("data-checked", "true");
    cartLink.addEventListener("click", (e) => {
      if (!checkAuth()) {
        e.preventDefault();
        alert("请先登录");
        window.location.href = "login.html";
      }
    });
  }

  if (orderLink && !orderLink.hasAttribute("data-checked")) {
    orderLink.setAttribute("data-checked", "true");
    orderLink.addEventListener("click", (e) => {
      if (!checkAuth()) {
        e.preventDefault();
        alert("请先登录");
        window.location.href = "login.html";
      }
    });
  }

  if (document.getElementById("productGrid")) {
    const catLinks = document.querySelectorAll(".pill[data-cat]");
    const setActive = (cat) => {
      catLinks.forEach((link) =>
        link.classList.toggle("active", link.getAttribute("data-cat") === cat)
      );
    };
    renderProducts("all");
    setActive("all");
    catLinks.forEach((a) => {
      a.addEventListener("click", (e) => {
        e.preventDefault();
        const cat = a.getAttribute("data-cat");
        renderProducts(cat);
        setActive(cat);
      });
    });
  }

  // 购物车页面：检查登录状态
  if (document.getElementById("cartBody")) {
    if (!checkAuth()) {
      alert("请先登录");
      window.location.href = "login.html";
    } else {
      loadCart();
    }
  }

  // 订单页面：检查登录状态并加载订单
  if (document.getElementById("orderBody")) {
    if (!checkAuth()) {
      alert("请先登录");
      window.location.href = "login.html";
    } else {
      loadOrders();
    }
  }

  // 购物车页面的结算按钮
  const checkoutBtn = document.querySelector('button[data-action="checkout"]');
  if (checkoutBtn) {
    checkoutBtn.addEventListener("click", createOrder);
  }

  // ------------ 登录表单处理 -------------
  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      e.stopPropagation();

      const identifier = document.getElementById("identifier").value.trim();
      const password = document.getElementById("password").value;

      if (!identifier || !password) {
        alert("请输入邮箱/手机号和密码");
        return false;
      }

      try {
        const response = await fetch(`${API_BASE}/auth/login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ identifier, password }),
        });

        if (!response.ok) {
          const errorText = await response.text();

          // 根据不同的错误类型显示对应的提示
          if (response.status === 404 || errorText.includes("用户不存在")) {
            // 用户不存在
            alert("用户不存在，请注册");
            window.location.href = "register.html";
            return false;
          } else if (
            response.status === 401 ||
            errorText.includes("密码错误")
          ) {
            // 密码错误
            alert("密码错误，请重新输入");
            return false;
          } else if (response.status === 400) {
            // 输入格式错误
            alert(errorText || "请输入正确的邮箱/手机号和密码");
            return false;
          } else {
            // 其他错误
            alert("登录失败：" + errorText);
            return false;
          }
        }

        const result = await response.json();
        // 保存token和用户信息到localStorage，用于后续的订单/购物车操作
        localStorage.setItem("token", result.token);
        localStorage.setItem("user", JSON.stringify(result.user));
        // 直接跳转到首页，不显示提示框
        window.location.href = "index.html";
        return false;
      } catch (err) {
        // 网络错误或其他异常
        alert("登录失败：" + err.message);
        return false;
      }
    });
  }

  // ------------ 注册表单处理 -------------
  const registerForm = document.getElementById("registerForm");
  if (registerForm) {
    registerForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      e.stopPropagation();

      const userName = document.getElementById("userName").value.trim();
      const email = document.getElementById("email").value.trim();
      const phone = document.getElementById("phone").value.trim();
      const password = document.getElementById("passwordR").value;
      const confirmPwd = document.getElementById("confirmPwd").value;

      if (!userName || !email || !password) {
        alert("用户名、邮箱和密码不能为空");
        return false;
      }

      if (password !== confirmPwd) {
        alert("两次输入的密码不一致");
        return false;
      }

      try {
        const result = await fetchJson(`${API_BASE}/auth/register`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            email,
            phone: phone || null,
            password,
            nickName: userName,
          }),
        });

        alert("注册成功！请登录");
        // 跳转到登录页面
        window.location.href = "login.html";
        return false;
      } catch (err) {
        alert("注册失败：" + err.message);
        return false;
      }
    });
  }
});
