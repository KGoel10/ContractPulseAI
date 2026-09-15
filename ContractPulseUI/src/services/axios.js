import axios from "axios";
import { API_BASE_URL } from "services/constants";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

const get = async (endpoint, config = {}) => {
  const response = await apiClient.get(endpoint, config);
  return response.data;
};

const post = async (endpoint, data = {}, config = {}) => {
  const response = await apiClient.post(endpoint, data, config);
  return response.data;
};

const put = async (endpoint, data = {}, config = {}) => {
  const response = await apiClient.put(endpoint, data, config);
  return response.data;
};

const remove = async (endpoint, config = {}) => {
  const response = await apiClient.delete(endpoint, config);
  return response.data;
};

const request = {
  get,
  post,
  put,
  delete: remove,
};

export { apiClient, get, post, put, remove as delete };
export default request;
