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
  if (!res.ok) throw new Error(await res.text());
  return res.json();
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
  {
    id: 101,
    name: "测试商品",
    price: 9.99,
    cat: "配件",
    img: "https://images.unsplash.com/photo-1585386959984-a4155224a1ad?w=260&h=180&fit=crop&auto=format&q=80"
  },
  {
    id: 102,
    name: "旗舰手机 X1",
    price: 3999,
    cat: "手机",
    // 使用 picsum 以保证国内可稳定加载
    img: "https://picsum.photos/seed/smartphone-x1/260/180"
  },
  {
    id: 103,
    name: "智能手表 S2",
    price: 899,
    cat: "智能穿戴",
    img: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=260&h=180&fit=crop&auto=format&q=80"
  },
  {
    id: 104,
    name: "轻薄笔记本 Pro",
    price: 5999,
    cat: "笔记本",
    img: "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=260&h=180&fit=crop&auto=format&q=80"
  }
];
function renderProducts(filterCat = "all") {
  const grid = document.getElementById("productGrid");
  if (!grid) return;
  grid.innerHTML = "";
  const isAll = !filterCat || filterCat === "all";
  demoProducts
    .filter((p) => isAll || p.cat === filterCat)
    .forEach((p) => {
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
  try {
    await fetchJson(`${API_BASE}/cart/add`, {
      method: "POST",
      headers: { ...authHeader(), "Content-Type": "application/json" },
      body: JSON.stringify({ productId: pid, quantity: qty })
    });
    alert("已加入购物车");
  } catch (err) {
    alert("请先登录");
    location.href = "login.html";
  }
}

// ------------ 购物车渲染 -------------
async function loadCart() {
  const cartBody = document.getElementById("cartBody");
  const cartTotalEl = document.getElementById("cartTotal");
  if (!cartBody) return;
  try {
    const items = await fetchJson(`${API_BASE}/cart`, { headers: authHeader() });
    cartBody.innerHTML = "";
    let total = 0;
    items.forEach((it) => {
      const sub = it.price * it.quantity;
      total += sub;
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${it.name}</td>
        <td>¥${it.price}</td>
        <td><button class="action-btn" data-type="dec" data-id="${it.productId}">-</button>
            ${it.quantity}
            <button class="action-btn" data-type="inc" data-id="${it.productId}">+</button></td>
        <td>¥${sub.toFixed(2)}</td>
        <td><button class="action-btn" data-type="del" data-id="${it.productId}">删除</button></td>`;
      cartBody.appendChild(tr);
    });
    cartTotalEl.textContent = `合计：¥${total.toFixed(2)}`;

    // 绑定事件
    cartBody.querySelectorAll("button[data-id]").forEach((btn) => {
      btn.addEventListener("click", async () => {
        const pid = Number(btn.getAttribute("data-id"));
        const type = btn.getAttribute("data-type");
        if (type === "del") {
          await fetchJson(`${API_BASE}/cart/${pid}`, { method: "DELETE", headers: authHeader() });
        } else {
          const delta = type === "inc" ? 1 : -1;
          await fetchJson(`${API_BASE}/cart/add`, {
            method: "POST",
            headers: { ...authHeader(), "Content-Type": "application/json" },
            body: JSON.stringify({ productId: pid, quantity: delta })
          });
        }
        loadCart();
      });
    });
  } catch (err) {
    alert("加载购物车失败，请先登录");
    location.href = "login.html";
  }
}

// ------------ 页面路由判断 -------------

document.addEventListener("DOMContentLoaded", () => {
  renderNav();

  if (document.getElementById("productGrid")) {
    const catLinks = document.querySelectorAll(".pill[data-cat]");
    const setActive = (cat) => {
      catLinks.forEach((link) => link.classList.toggle("active", link.getAttribute("data-cat") === cat));
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

  if (document.getElementById("cartBody")) {
    loadCart();
  }

  // 登录/注册表单逻辑复用现有实现
});