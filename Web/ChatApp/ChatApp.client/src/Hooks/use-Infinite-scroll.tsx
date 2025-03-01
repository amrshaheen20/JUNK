import { useCallback, useEffect, useRef, useState } from "react";

type InfiniteScrollProps = {
  ContainerRef: React.RefObject<HTMLDivElement>;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  fetchNextPage: () => void;
  fetchPreviousPage: () => void;
  isFetching: boolean;
  Invert?: boolean;
};

export const useInfiniteScroll = ({
  ContainerRef,
  hasNextPage,
  hasPreviousPage,
  fetchNextPage,
  fetchPreviousPage,
  isFetching,
  Invert = false,
}: InfiniteScrollProps) => {
  const scrollPositionRef = useRef<number>(0);
  const [isFetchingTop, setIsFetchingTop] = useState<boolean>(false);
  const [scrollValue, setScrollValue] = useState<number>(0);
  const scrollThreshold = 100;
  const handleScroll = useCallback(() => {
    const container = ContainerRef.current;
    if (!container || isFetching) return;

    const { scrollTop, scrollHeight, clientHeight } = container;
    setScrollValue(scrollTop);
    scrollPositionRef.current = Invert
      ? scrollHeight - clientHeight - scrollThreshold
      : scrollHeight;

    const atBottom =
      Math.abs(scrollTop) + clientHeight >= scrollHeight - scrollThreshold;
    const atTop = Math.abs(scrollTop) <= scrollThreshold;

    if (atTop && hasPreviousPage) {
      fetchPreviousPage();
      setIsFetchingTop(true);
    } else if (atBottom && hasNextPage) {
      fetchNextPage();
    }
  }, [
    ContainerRef,
    isFetching,
    Invert,
    hasPreviousPage,
    hasNextPage,
    fetchPreviousPage,
    fetchNextPage,
  ]);

  useEffect(() => {
    const container = ContainerRef.current;
    if (!container) return;

    container.addEventListener("scroll", handleScroll);
    return () => container.removeEventListener("scroll", handleScroll);
  }, [ContainerRef, handleScroll]);

  useEffect(() => {
    const container = ContainerRef.current;
    if (!container || isFetching) return;

    if (isFetchingTop) {
      container.scrollTop = Invert
        ? -(container.scrollHeight - scrollPositionRef.current)
        : container.scrollHeight - scrollPositionRef.current;
      setIsFetchingTop(false);
    }
  }, [ContainerRef, Invert, isFetchingTop, isFetching]);

  const scrollToTop = () => {
    const container = ContainerRef.current;
    if (container) {
      container.scrollTo({
        top: !Invert ? -container.scrollHeight : 0,
        behavior: "smooth",
      });
    }
  };

  const scrollToBottom = () => {
    const container = ContainerRef.current;
    if (container) {
      container.scrollTo({
        top: !Invert ? 0 : -container.scrollHeight,
        behavior: "smooth",
      });
    }
  };
  return { scrollToTop, scrollToBottom, scrollValue };
};
